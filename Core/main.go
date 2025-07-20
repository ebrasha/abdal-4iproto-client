// -------------------------------------------------------------------
// Programmer       : Ebrahim Shafiei (EbraSha)
// Email            : Prof.Shafiei@Gmail.com
// -------------------------------------------------------------------

package main

import (
	"bufio"
	"bytes"
	"encoding/binary"
	"encoding/json"
	"flag"
	"fmt"
	tea "github.com/charmbracelet/bubbletea"
	"github.com/charmbracelet/lipgloss"
	"golang.org/x/crypto/ssh"
	"io"
	"net"
	"net/http"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"sync"
	"syscall"
	"time"
	"unsafe"
)

var pushEnabled bool
var pushEndpoint string
var (
	currentSSHClient *ssh.Client
	sshClientMutex   sync.RWMutex
)

// TrafficLog is used to report traffic stats to optional log server
type TrafficLog struct {
	Username      string `json:"username"`       // From config.SSHUser
	RemoteIP      string `json:"ip"`             // Optional - can be empty for AppLog
	BytesSent     int64  `json:"bytes_sent"`     // 0 if not traffic
	BytesReceived int64  `json:"bytes_received"` // 0 if not traffic
	TotalBytes    int64  `json:"total_bytes"`    // 0 if not traffic
	Timestamp     string `json:"timestamp"`      // Always filled
	Message       string `json:"message"`        // Optional log message
	Level         string `json:"level"`          // "INFO", "ERROR", etc.
}

var (
	styleBanner       = lipgloss.NewStyle().Foreground(lipgloss.Color("#00fd8d"))
	styleSuccess      = lipgloss.NewStyle().Foreground(lipgloss.Color("#00fe94"))
	styleError        = lipgloss.NewStyle().Foreground(lipgloss.Color("9"))
	styleInfo         = lipgloss.NewStyle().Foreground(lipgloss.Color("#15e5f2"))
	styleInfoProxyReq = lipgloss.NewStyle().Foreground(lipgloss.Color("#b437fd"))
	styleWarn         = lipgloss.NewStyle().Foreground(lipgloss.Color("11"))
)

type logMsg string

type Config struct {
	SSHHost              string `json:"ssh_host"`
	SSHPort              int    `json:"ssh_port"`
	SSHUser              string `json:"ssh_user"`
	SSHPassword          string `json:"ssh_password"`
	Socks5Port           int    `json:"socks5_port"`
	AutoReconnect        string `json:"auto_reconnect"`
	AutoReconnectTimeout int    `json:"auto_reconnect_timeout"`
}

type model struct {
	cfg     *Config
	domains []string
	log     []string
	logChan chan tea.Msg
}

// PushTrafficLog sends traffic info as JSON to configured web server
func PushTrafficLog(log TrafficLog) {
	if !pushEnabled {
		return
	}
	data, err := json.Marshal(log)
	if err != nil {
		return
	}
	_, _ = http.Post(pushEndpoint, "application/json", bytes.NewBuffer(data))
}

func (m model) Init() tea.Cmd {
	return startSSH(m.cfg, &m)
}

func (m model) Update(msg tea.Msg) (tea.Model, tea.Cmd) {
	switch v := msg.(type) {
	case logMsg:
		m.log = append(m.log, string(v))
	}
	return m, nil
}

func (m model) View() string {
	banner := `

░█████╗░██████╗░██████╗░░█████╗░██╗░░░░░  ░░██╗██╗██╗██████╗░██████╗░░█████╗░████████╗░█████╗░
██╔══██╗██╔══██╗██╔══██╗██╔══██╗██║░░░░░  ░██╔╝██║██║██╔══██╗██╔══██╗██╔══██╗╚══██╔══╝██╔══██╗
███████║██████╦╝██║░░██║███████║██║░░░░░  ██╔╝░██║██║██████╔╝██████╔╝██║░░██║░░░██║░░░██║░░██║
██╔══██║██╔══██╗██║░░██║██╔══██║██║░░░░░  ███████║██║██╔═══╝░██╔══██╗██║░░██║░░░██║░░░██║░░██║
██║░░██║██████╦╝██████╔╝██║░░██║███████╗  ╚════██║██║██║░░░░░██║░░██║╚█████╔╝░░░██║░░░╚█████╔╝
╚═╝░░╚═╝╚═════╝░╚═════╝░╚═╝░░╚═╝╚══════╝  ░░░░░╚═╝╚═╝╚═╝░░░░░╚═╝░░╚═╝░╚════╝░░░░╚═╝░░░░╚════╝░

░█████╗░██╗░░░░░██╗███████╗███╗░░██╗████████╗
██╔══██╗██║░░░░░██║██╔════╝████╗░██║╚══██╔══╝
██║░░╚═╝██║░░░░░██║█████╗░░██╔██╗██║░░░██║░░░
██║░░██╗██║░░░░░██║██╔══╝░░██║╚████║░░░██║░░░
╚█████╔╝███████╗██║███████╗██║░╚███║░░░██║░░░
░╚════╝░╚══════╝╚═╝╚══════╝╚═╝░░╚══╝░░░╚═╝░░░

Abdal 4iProto Client ver 5.75
`
	view := styleBanner.Render(banner) + "\n"
	view += styleBanner.Render("Programmer: Ebrahim Shafiei (EbraSha)") + "\n"
	view += styleBanner.Render("Github: https://github.com/ebrasha)") + "\n"
	view += styleBanner.Render("This software is part of the Abdal arsenal, which belongs to the Abdal Security Group, led by Ebrahim Shafiei (EbraSha).") + "\n"
	view += styleInfo.Render("----------------------------------------------") + "\n"
	for _, line := range m.log {
		view += line + "\n"
	}
	return view
}

func LoadConfig(path string) (*Config, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil, err
	}
	var cfg Config
	err = json.Unmarshal(data, &cfg)
	return &cfg, err
}

func maintainSSHConnection(cfg *Config, m *model) {
	for {
		addr := fmt.Sprintf("%s:%d", cfg.SSHHost, cfg.SSHPort)
		client, err := ssh.Dial("tcp", addr, &ssh.ClientConfig{
			User:            cfg.SSHUser,
			Auth:            []ssh.AuthMethod{ssh.Password(cfg.SSHPassword)},
			HostKeyCallback: ssh.InsecureIgnoreHostKey(),
			Timeout:         5 * time.Second,
		})
		if err != nil {
			//m.logChan <- logMsg(styleError.Render("[ERROR] SSH connect failed: " + err.Error()))
			logAndPush(m, "ERROR", "SSH connect failed: "+err.Error(), styleError.Render("[ERROR] SSH connect failed: "+err.Error()))

			time.Sleep(time.Duration(cfg.AutoReconnectTimeout) * time.Millisecond)
			continue
		}

		sshClientMutex.Lock()
		currentSSHClient = client
		sshClientMutex.Unlock()

		//m.logChan <- logMsg(styleSuccess.Render("[OK] SSH connection established and monitoring"))
		logAndPush(m, "SUCCESS", "SSH connection established and monitoring", styleSuccess.Render("[OK] SSH connection established and monitoring"))

		for {
			_, _, err := client.SendRequest("keepalive@openssh.com", true, nil)
			if err != nil {
				//m.logChan <- logMsg(styleWarn.Render("[WARN] SSH connection lost. Reconnecting..."))
				logAndPush(m, "WARN", "SSH connection lost. Reconnecting...", styleWarn.Render("[WARN] SSH connection lost. Reconnecting..."))

				break
			}
			time.Sleep(5 * time.Second)
		}
	}
}

func startSSH(cfg *Config, m *model) tea.Cmd {
	return func() tea.Msg {
		go maintainSSHConnection(cfg, m)
		StartSOCKS5(cfg, nil, m)
		return nil
	}
}

func StartSOCKS5(cfg *Config, client *ssh.Client, m *model) {
	addr := fmt.Sprintf("127.0.0.1:%d", cfg.Socks5Port)
	//m.logChan <- logMsg(styleInfo.Render("[INFO] Initializing SOCKS5 listener at ") + addr)
	msg := "Initializing SOCKS5 listener at " + addr
	logAndPush(m, "INFO", msg, styleInfo.Render("[INFO] "+msg))

	ln, err := net.Listen("tcp", addr)
	if err != nil {
		//m.logChan <- logMsg(styleError.Render("[ERROR] Failed to bind SOCKS5: ") + err.Error())
		msg := "Failed to bind SOCKS5: " + err.Error()
		logAndPush(m, "ERROR", msg, styleError.Render("[ERROR] "+msg))

		return
	}
	//m.logChan <- logMsg(styleSuccess.Render("[OK] SOCKS5 proxy listening on ") + addr)
	msg = "SOCKS5 proxy listening on " + addr
	logAndPush(m, "SUCCESS", msg, styleSuccess.Render("[OK] "+msg))

	for {
		conn, err := ln.Accept()
		if err != nil {
			//m.logChan <- logMsg(styleError.Render("[ERROR] Listener accept error: ") + err.Error())
			logAndPush(m, "ERROR", "Listener accept error: "+err.Error(), styleError.Render("[ERROR] Listener accept error: "+err.Error()))

			continue
		}
		//m.logChan <- logMsg(styleInfo.Render("[INFO] New SOCKS5 client connection from ") + conn.RemoteAddr().String())
		logAndPush(m, "INFO", "New SOCKS5 client connection from "+conn.RemoteAddr().String(), styleInfo.Render("[INFO] New SOCKS5 client connection from "+conn.RemoteAddr().String()))

		go handleClient(conn, client, m)
	}
}

func getSSHClient() *ssh.Client {
	sshClientMutex.RLock()
	defer sshClientMutex.RUnlock()
	return currentSSHClient
}

func handleClient(conn net.Conn, _ *ssh.Client, m *model) {
	client := getSSHClient()
	if client == nil {
		//m.logChan <- logMsg(styleError.Render("[ERROR] No active SSH client available"))
		logAndPush(m, "ERROR", "No active SSH client available", styleError.Render("[ERROR] No active SSH client available"))

		return
	}

	defer conn.Close()
	buf := make([]byte, 262)

	// SOCKS5 handshake
	if _, err := io.ReadFull(conn, buf[:2]); err != nil {
		return
	}
	nMethods := int(buf[1])
	if _, err := io.ReadFull(conn, buf[:nMethods]); err != nil {
		return
	}
	// No authentication
	conn.Write([]byte{0x05, 0x00})

	// Request
	if _, err := io.ReadFull(conn, buf[:4]); err != nil {
		return
	}
	if buf[1] != 0x01 {
		// Only CONNECT supported
		conn.Write([]byte{0x05, 0x07, 0x00, 0x01})
		return
	}

	// Parse destination address
	var destHost string
	var destPort uint16
	switch buf[3] {
	case 0x01: // IPv4
		if _, err := io.ReadFull(conn, buf[:6]); err != nil {
			return
		}
		destHost = net.IP(buf[:4]).String()
		destPort = binary.BigEndian.Uint16(buf[4:6])
	case 0x03: // Domain
		if _, err := io.ReadFull(conn, buf[:1]); err != nil {
			return
		}
		dlen := int(buf[0])
		if _, err := io.ReadFull(conn, buf[:dlen+2]); err != nil {
			return
		}
		destHost = string(buf[:dlen])
		destPort = binary.BigEndian.Uint16(buf[dlen : dlen+2])
	default:
		// Address type not supported
		conn.Write([]byte{0x05, 0x08, 0x00, 0x01})
		return
	}

	// Send initial SOCKS5 success reply (bind address unused)
	reply := []byte{0x05, 0x00, 0x00, 0x01}
	reply = append(reply, make([]byte, 6)...) // 4-byte addr + 2-byte port
	conn.Write(reply)

	// Bypass logic: direct connect if domain matches patterns
	if shouldBypass(destHost, m.domains) {
		//m.logChan <- logMsg(styleWarn.Render("[BYPASS] Direct connect to ") + fmt.Sprintf("%s:%d", destHost, destPort))
		logAndPush(m, "WARN", fmt.Sprintf("Direct connect to %s:%d", destHost, destPort), styleWarn.Render("[BYPASS] Direct connect to "+fmt.Sprintf("%s:%d", destHost, destPort)))

		direct, err := net.Dial("tcp", fmt.Sprintf("%s:%d", destHost, destPort))
		if err != nil {
			//m.logChan <- logMsg(styleError.Render("[ERROR] Direct connection failed: ") + err.Error())
			logAndPush(m, "ERROR", "Direct connection failed: "+err.Error(), styleError.Render("[ERROR] Direct connection failed: "+err.Error()))

			return
		}
		defer direct.Close()
		go io.Copy(direct, conn)
		io.Copy(conn, direct)
		return
	}

	// Otherwise, open SSH tunnel channel
	type channelOpenDirect struct {
		DestAddr string
		DestPort uint32
		SrcAddr  string
		SrcPort  uint32
	}
	payload := ssh.Marshal(&channelOpenDirect{
		DestAddr: destHost,
		DestPort: uint32(destPort),
		SrcAddr:  "127.0.0.1",
		SrcPort:  0,
	})
	ch, reqs, err := client.OpenChannel("direct-tcpip", payload)
	if err != nil {
		//m.logChan <- logMsg(styleError.Render("[ERROR] Failed to connect via Abdal 4iProto Client to ") + fmt.Sprintf("%s:%d", destHost, destPort))
		logAndPush(m, "ERROR", fmt.Sprintf("Failed to connect via Abdal 4iProto Client to %s:%d", destHost, destPort), styleError.Render("[ERROR] Failed to connect via Abdal 4iProto Client to "+fmt.Sprintf("%s:%d", destHost, destPort)))

		conn.Write([]byte{0x05, 0x05, 0x00, 0x01}) // connection refused
		return
	}
	go ssh.DiscardRequests(reqs)

	// Log and proxy data over SSH channel
	//m.logChan <- logMsg(styleInfoProxyReq.Render("[INFO] Proxying request to ") + fmt.Sprintf("%s:%d via Abdal 4iProto Client", destHost, destPort))
	logAndPush(m, "INFO", fmt.Sprintf("Proxying request to %s:%d via Abdal 4iProto Client", destHost, destPort), styleInfoProxyReq.Render("[INFO] Proxying request to "+fmt.Sprintf("%s:%d via Abdal 4iProto Client", destHost, destPort)))

	var sentBytes, recvBytes int64

	go func() {
		n, _ := io.Copy(ch, conn)
		sentBytes = n
	}()
	recvBytes, _ = io.Copy(conn, ch)

	PushTrafficLog(TrafficLog{
		Username:      m.cfg.SSHUser,
		RemoteIP:      conn.RemoteAddr().String(),
		BytesSent:     sentBytes,
		BytesReceived: recvBytes,
		TotalBytes:    sentBytes + recvBytes,
		Timestamp:     time.Now().Format(time.RFC3339),
		Message:       "Traffic session completed",
		Level:         "INFO",
	})

}

// SetConsoleTitle sets the terminal window title across platforms
func SetConsoleTitle(title string) {
	switch runtime.GOOS {
	case "windows":
		// Use Windows API to set console title
		ptr := syscall.StringToUTF16Ptr(title)
		kernel32 := syscall.NewLazyDLL("kernel32.dll")
		setConsoleTitle := kernel32.NewProc("SetConsoleTitleW")
		setConsoleTitle.Call(uintptr(unsafe.Pointer(ptr)))
	default:
		// For Linux/macOS, use ANSI escape sequence
		fmt.Printf("\033]0;%s\007", title)
	}
}

// LoadDomains reads wildcard/domain patterns from domains.txt
func LoadDomains(path string) ([]string, error) {
	f, err := os.Open(path)
	if err != nil {
		return nil, err
	}
	defer f.Close()

	var domains []string
	scanner := bufio.NewScanner(f)
	for scanner.Scan() {
		line := strings.TrimSpace(scanner.Text())
		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}
		domains = append(domains, line)
	}
	return domains, scanner.Err()
}

// shouldBypass returns true if host matches any pattern in domains
func shouldBypass(host string, patterns []string) bool {
	host = strings.ToLower(host)
	for _, p := range patterns {
		p = strings.ToLower(p)
		if strings.HasPrefix(p, "*.") {
			// wildcard suffix, e.g. *.ir
			if strings.HasSuffix(host, p[1:]) {
				return true
			}
		} else if host == p {
			return true
		}
	}
	return false
}

func logAndPush(m *model, level string, msg string, styled string) {
	m.logChan <- logMsg(styled)
	if !pushEnabled {
		return // don't push to server if -p was not set
	}
	logEntry := TrafficLog{
		Username:      m.cfg.SSHUser,
		RemoteIP:      "", // only used in traffic handler
		BytesSent:     0,
		BytesReceived: 0,
		TotalBytes:    0,
		Timestamp:     time.Now().Format(time.RFC3339),
		Message:       msg,
		Level:         level,
	}
	data, err := json.Marshal(logEntry)
	if err != nil {
		return
	}
	_, _ = http.Post(pushEndpoint, "application/json", bytes.NewBuffer(data))
}

func main() {
	SetConsoleTitle("Abdal 4iProto Client 🔐")

	// Parse CLI flags
	var (
		configPath = flag.String("c", "", "Path to config.json")
		pushPort   = flag.String("p", "", "Push log to http://127.0.0.1:<port>")
	)
	flag.Parse()

	var cfgPath string
	if *configPath != "" {
		cfgPath = *configPath
	} else {
		//Default: config.json file is located next to the application.
		exePath, err := os.Executable()
		if err != nil {
			fmt.Println(styleError.Render("[ERROR] Failed to determine executable path: ") + err.Error())
			os.Exit(1)
		}
		exeDir := filepath.Dir(exePath)
		cfgPath = filepath.Join(exeDir, "config.json")
	}

	cfg, err := LoadConfig(cfgPath)
	if err != nil {
		fmt.Println(styleError.Render("[ERROR] Could not read config: ") + err.Error())
		os.Exit(1)
	}

	domains, err := LoadDomains("domains.txt")
	if err != nil {
		fmt.Println("Failed to load domains.txt:", err)
		os.Exit(1)
	}
	logChan := make(chan tea.Msg, 100)

	if *pushPort != "" {
		pushEnabled = true
		pushEndpoint = fmt.Sprintf("http://127.0.0.1:%s/log", *pushPort)
	}

	p := tea.NewProgram(model{
		cfg:     cfg,
		domains: domains,
		logChan: logChan})
	go func() {
		for msg := range logChan {
			p.Send(msg)
		}
	}()
	defer close(logChan)
	if _, err := p.Run(); err != nil {
		fmt.Println(styleError.Render("[ERROR] BubbleTea error: ") + err.Error())
		os.Exit(1)
	}
}
