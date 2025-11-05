/*
 **********************************************************************
 * -------------------------------------------------------------------
 * Project Name : Abdal 4iProto Client
 * File Name    : main.go
 * Author       : Ebrahim Shafiei (EbraSha)
 * Email        : Prof.Shafiei@Gmail.com
 * Created On   : 2025-01-27 10:30:00
 * Description  : Main application file for Abdal 4iProto Client with SOCKS5 proxy, SSH tunneling, UDP support, domain bypass, and ad blocking functionality
 * -------------------------------------------------------------------
 *
 * "Coding is an engaging and beloved hobby for me. I passionately and insatiably pursue knowledge in cybersecurity and programming."
 * – Ebrahim Shafiei
 *
 **********************************************************************
 */

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

// UDP Helper structures and functions for direct-udpip support
type directUDPIPReq struct {
	HostToConnect     string
	PortToConnect     uint32
	OriginatorAddress string
	OriginatorPort    uint32
}

type udpChan struct {
	ch        ssh.Channel
	lastUsed  time.Time
	lastAddr  *net.UDPAddr
	closeOnce sync.Once
}

type udpChanCache struct {
	mu      sync.Mutex
	entries map[string]*udpChan
	ttl     time.Duration
}

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

// UDP Helper Functions
func openUDPChannel(cl *ssh.Client, host string, port uint32) (ssh.Channel, <-chan *ssh.Request, error) {
	req := directUDPIPReq{
		HostToConnect:     host,
		PortToConnect:     port,
		OriginatorAddress: "0.0.0.0",
		OriginatorPort:    0,
	}
	extra := ssh.Marshal(&req)
	ch, reqs, err := cl.OpenChannel("direct-udpip", extra)
	return ch, reqs, err
}

func sendUDPDatagram(ch ssh.Channel, payload []byte) error {
	if len(payload) > 65535 {
		return fmt.Errorf("oversize datagram")
	}
	var lb [2]byte
	binary.BigEndian.PutUint16(lb[:], uint16(len(payload)))
	if _, err := ch.Write(lb[:]); err != nil {
		return err
	}
	_, err := ch.Write(payload)
	return err
}

func recvUDPDatagram(r io.Reader) ([]byte, error) {
	var lb [2]byte
	if _, err := io.ReadFull(r, lb[:]); err != nil {
		return nil, err
	}
	n := int(binary.BigEndian.Uint16(lb[:]))
	if n <= 0 || n > 65535 {
		return nil, fmt.Errorf("bad length")
	}
	b := make([]byte, n)
	_, err := io.ReadFull(r, b)
	return b, err
}

func parseSocks5UDP(b []byte) (string, uint16, []byte, error) {
	if len(b) < 4 {
		return "", 0, nil, fmt.Errorf("short packet")
	}
	if b[0] != 0x00 || b[1] != 0x00 {
		return "", 0, nil, fmt.Errorf("bad rsv")
	}
	if b[2] != 0x00 {
		return "", 0, nil, fmt.Errorf("frag unsupported")
	}
	atyp := b[3]
	p := 4
	var host string
	switch atyp {
	case 0x01: // IPv4
		if len(b) < p+4+2 {
			return "", 0, nil, fmt.Errorf("short ipv4")
		}
		host = net.IP(b[p : p+4]).String()
		p += 4
	case 0x03: // Domain
		if len(b) < p+1 {
			return "", 0, nil, fmt.Errorf("short domain len")
		}
		dlen := int(b[p])
		p++
		if len(b) < p+dlen+2 {
			return "", 0, nil, fmt.Errorf("short domain")
		}
		host = string(b[p : p+dlen])
		p += dlen
	case 0x04: // IPv6
		if len(b) < p+16+2 {
			return "", 0, nil, fmt.Errorf("short ipv6")
		}
		host = net.IP(b[p : p+16]).String()
		p += 16
	default:
		return "", 0, nil, fmt.Errorf("bad atyp")
	}
	if len(b) < p+2 {
		return "", 0, nil, fmt.Errorf("short port")
	}
	port := binary.BigEndian.Uint16(b[p : p+2])
	p += 2
	if len(b) < p {
		return "", 0, nil, fmt.Errorf("short payload")
	}
	return host, port, b[p:], nil
}

func buildSocks5UDP(host string, port uint16, payload []byte) ([]byte, error) {
	buf := bytes.NewBuffer(nil)
	buf.Write([]byte{0x00, 0x00})
	buf.WriteByte(0x00)
	ip := net.ParseIP(host)
	if ip4 := ip.To4(); ip4 != nil {
		buf.WriteByte(0x01)
		buf.Write(ip4.To4())
	} else if ip != nil && ip.To16() != nil {
		buf.WriteByte(0x04)
		buf.Write(ip.To16())
	} else {
		if len(host) > 255 {
			return nil, fmt.Errorf("domain too long")
		}
		buf.WriteByte(0x03)
		buf.WriteByte(byte(len(host)))
		buf.WriteString(host)
	}
	var p [2]byte
	binary.BigEndian.PutUint16(p[:], port)
	buf.Write(p[:])
	buf.Write(payload)
	return buf.Bytes(), nil
}

func newUDPChanCache(ttl time.Duration) *udpChanCache {
	return &udpChanCache{entries: make(map[string]*udpChan), ttl: ttl}
}

func (c *udpChanCache) getOrCreate(key string, dial func() (*udpChan, error)) (*udpChan, error) {
	c.mu.Lock()
	defer c.mu.Unlock()
	if uc, ok := c.entries[key]; ok {
		uc.lastUsed = time.Now()
		return uc, nil
	}
	uc, err := dial()
	if err != nil {
		return nil, err
	}
	uc.lastUsed = time.Now()
	c.entries[key] = uc
	return uc, nil
}

func (c *udpChanCache) janitor() {
	t := time.NewTicker(c.ttl / 2)
	defer t.Stop()
	for range t.C {
		now := time.Now()
		var stale []string
		c.mu.Lock()
		for k, uc := range c.entries {
			if now.Sub(uc.lastUsed) > c.ttl {
				stale = append(stale, k)
			}
		}
		for _, k := range stale {
			uc := c.entries[k]
			delete(c.entries, k)
			uc.closeOnce.Do(func() { _ = uc.ch.Close() })
		}
		c.mu.Unlock()
	}
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
	SSHHost                    string `json:"ssh_host"`
	SSHPort                    int    `json:"ssh_port"`
	SSHUser                    string `json:"ssh_user"`
	SSHPassword                string `json:"ssh_password"`
	Socks5Port                 int    `json:"socks5_port"`
	AutoReconnect              string `json:"auto_reconnect"`
	AutoReconnectTimeout       int    `json:"auto_reconnect_timeout"`
	AdBlockingLog              string `json:"ad_blocking_log"`
	AdBlockingLogMaxSizeMB     int    `json:"ad_blocking_log_max_size_mb"`
}

type model struct {
	cfg       *Config
	domains   []string
	blockedAds []string
	log       []string
	logChan   chan tea.Msg
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

Abdal 4iProto Client ver 6.25
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
			ClientVersion: "SSH-2.0-Abdal-4iProtoClient",
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
	
	// Log ad blocking status
	if m.cfg.AdBlockingLog == "yes" {
		adBlockMsg := fmt.Sprintf("Ad blocking enabled - logs will be saved to ad_blocking.log (max size: %d MB)", m.cfg.AdBlockingLogMaxSizeMB)
		logAndPush(m, "INFO", adBlockMsg, styleInfo.Render("[INFO] "+adBlockMsg))
	}

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
	
	// Handle UDP ASSOCIATE command (0x03)
	if buf[1] == 0x03 {
		// Bind a local UDP socket to receive UDP packets from the client application
		udpAddr, _ := net.ResolveUDPAddr("udp", "127.0.0.1:0")
		udpConn, err := net.ListenUDP("udp", udpAddr)
		if err != nil {
			// reply: general failure
			conn.Write([]byte{0x05, 0x01, 0x00, 0x01, 0, 0, 0, 0, 0, 0})
			return
		}
		defer udpConn.Close()

		// Send success reply with BND.ADDR/BND.PORT of our UDP bind
		la := udpConn.LocalAddr().(*net.UDPAddr)
		reply := bytes.NewBuffer(nil)
		reply.WriteByte(0x05)        // VER
		reply.WriteByte(0x00)        // REP = succeeded
		reply.WriteByte(0x00)        // RSV
		reply.WriteByte(0x01)        // ATYP = IPv4 (using 127.0.0.1)
		ip4 := la.IP.To4()
		if ip4 == nil {
			ip4 = net.IPv4(127, 0, 0, 1)
		}
		reply.Write(ip4)
		var p2 [2]byte
		binary.BigEndian.PutUint16(p2[:], uint16(la.Port))
		reply.Write(p2[:])
		conn.Write(reply.Bytes())

		// Log UDP ASSOCIATE request
		logAndPush(m, "INFO", "UDP ASSOCIATE request received", styleInfo.Render("[INFO] UDP ASSOCIATE request received"))

		// Create UDP channel cache for efficient connection reuse
		cache := newUDPChanCache(90 * time.Second)
		go cache.janitor()

		// Relay loop: UDP <-> SSH "direct-udpip"
		// Keep TCP control connection alive
		go func() {
			io.Copy(io.Discard, conn) // keep TCP control alive
		}()

		bufUDP := make([]byte, 65535)
		for {
			udpConn.SetReadDeadline(time.Now().Add(180 * time.Second))
			n, clientAddr, err := udpConn.ReadFromUDP(bufUDP)
			if err != nil {
				return
			}
			host, port, payload, err := parseSocks5UDP(bufUDP[:n])
			if err != nil {
				continue
			}
			key := fmt.Sprintf("%s:%d", host, port)

			uc, err := cache.getOrCreate(key, func() (*udpChan, error) {
				ch, reqs, err := openUDPChannel(client, host, uint32(port))
				if err != nil {
					return nil, err
				}
				go ssh.DiscardRequests(reqs)
				u := &udpChan{ch: ch}
				go func(dest string, uch *udpChan) {
					for {
						// Use a timeout channel for reading
						timeout := time.After(120 * time.Second)
						dataChan := make(chan []byte, 1)
						errChan := make(chan error, 1)
						
						go func() {
							data, err := recvUDPDatagram(uch.ch)
							if err != nil {
								errChan <- err
								return
							}
							dataChan <- data
						}()
						
						select {
						case data := <-dataChan:
							uchAddr := func() *net.UDPAddr {
								cache.mu.Lock()
								defer cache.mu.Unlock()
								if x, ok := cache.entries[dest]; ok {
									return x.lastAddr
								}
								return nil
							}()
							if uchAddr != nil {
								pkt, e := buildSocks5UDP(host, port, data)
								if e == nil {
									_, _ = udpConn.WriteToUDP(pkt, uchAddr)
								}
							}
						case <-errChan:
							uch.closeOnce.Do(func() { _ = uch.ch.Close() })
							return
						case <-timeout:
							uch.closeOnce.Do(func() { _ = uch.ch.Close() })
							return
						}
					}
				}(key, u)
				return u, nil
			})
			if err != nil {
				continue
			}

			cache.mu.Lock()
			uc.lastAddr = clientAddr
			uc.lastUsed = time.Now()
			cache.mu.Unlock()

			// Send UDP datagram with timeout using goroutine
			sendChan := make(chan error, 1)
			go func() {
				sendChan <- sendUDPDatagram(uc.ch, payload)
			}()
			
			select {
			case err := <-sendChan:
				if err != nil {
					uc.closeOnce.Do(func() { _ = uc.ch.Close() })
					cache.mu.Lock()
					delete(cache.entries, key)
					cache.mu.Unlock()
					continue
				}
			case <-time.After(10 * time.Second):
				uc.closeOnce.Do(func() { _ = uc.ch.Close() })
				cache.mu.Lock()
				delete(cache.entries, key)
				cache.mu.Unlock()
				continue
			}
		}
		return
	}

	if buf[1] != 0x01 {
		// Only CONNECT and UDP ASSOCIATE supported
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

	// Ad blocking logic: block if domain matches blocked patterns
	if shouldBlock(destHost, m.blockedAds) {
		blockMsg := fmt.Sprintf("Ad blocked: %s:%d", destHost, destPort)
		logAndPush(m, "BLOCK", blockMsg, styleError.Render("[BLOCK] "+blockMsg))
		
		// Log blocked traffic if enabled
		if m.cfg.AdBlockingLog == "yes" {
			// Log to file with full date and time
			logAdBlockToFile(conn.RemoteAddr().String(), destHost, destPort)
			
			// Also push to traffic log server if enabled
			PushTrafficLog(TrafficLog{
				Username:      m.cfg.SSHUser,
				RemoteIP:      conn.RemoteAddr().String(),
				BytesSent:     0,
				BytesReceived: 0,
				TotalBytes:    0,
				Timestamp:     time.Now().Format(time.RFC3339),
				Message:       "Ad traffic blocked: " + destHost,
				Level:         "BLOCK",
			})
		}
		
		// Send connection refused response
		conn.Write([]byte{0x05, 0x05, 0x00, 0x01})
		return
	}

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

// LoadBlockedAds reads blocked ad domains and IPs from ads.txt
func LoadBlockedAds(path string) ([]string, error) {
	f, err := os.Open(path)
	if err != nil {
		return nil, err
	}
	defer f.Close()

	var blockedAds []string
	scanner := bufio.NewScanner(f)
	for scanner.Scan() {
		line := strings.TrimSpace(scanner.Text())
		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}
		blockedAds = append(blockedAds, line)
	}
	return blockedAds, scanner.Err()
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

// shouldBlock returns true if host matches any pattern in blocked ads
func shouldBlock(host string, blockedPatterns []string) bool {
	host = strings.ToLower(host)
	for _, p := range blockedPatterns {
		p = strings.ToLower(p)
		if strings.HasPrefix(p, "*.") {
			// wildcard suffix, e.g. *.facebook.com
			if strings.HasSuffix(host, p[1:]) {
				return true
			}
		} else if host == p {
			return true
		}
	}
	return false
}

// checkAndManageLogFileSize checks if ad_blocking.log exceeds the maximum size and deletes it if necessary
func checkAndManageLogFileSize(maxSizeMB int) {
	// Get executable directory to place log file next to the application
	exePath, err := os.Executable()
	if err != nil {
		return
	}
	exeDir := filepath.Dir(exePath)
	logFilePath := filepath.Join(exeDir, "ad_blocking.log")
	
	// Check if file exists
	fileInfo, err := os.Stat(logFilePath)
	if err != nil {
		// File doesn't exist, nothing to do
		return
	}
	
	// Calculate file size in MB
	fileSizeMB := fileInfo.Size() / (1024 * 1024)
	
	// If file size exceeds the maximum, delete it
	if fileSizeMB >= int64(maxSizeMB) {
		err = os.Remove(logFilePath)
		if err != nil {
			// Silently fail if we can't delete the file
			return
		}
	}
}

// logAdBlockToFile writes ad blocking logs to a text file
func logAdBlockToFile(remoteIP, blockedHost string, destPort uint16) {
	// Get executable directory to place log file next to the application
	exePath, err := os.Executable()
	if err != nil {
		return
	}
	exeDir := filepath.Dir(exePath)
	logFilePath := filepath.Join(exeDir, "ad_blocking.log")
	
	// Create log entry with full date and time
	timestamp := time.Now().Format("2006-01-02 15:04:05")
	logEntry := fmt.Sprintf("[%s] BLOCKED: %s:%d from %s\n", timestamp, blockedHost, destPort, remoteIP)
	
	// Open file in append mode, create if doesn't exist
	file, err := os.OpenFile(logFilePath, os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0644)
	if err != nil {
		return
	}
	defer file.Close()
	
	// Write log entry
	_, err = file.WriteString(logEntry)
	if err != nil {
		return
	}
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

	// Check and manage ad blocking log file size at startup
	if cfg.AdBlockingLog == "yes" && cfg.AdBlockingLogMaxSizeMB > 0 {
		checkAndManageLogFileSize(cfg.AdBlockingLogMaxSizeMB)
	}

	domains, err := LoadDomains("domains.txt")
	if err != nil {
		fmt.Println("Failed to load domains.txt:", err)
		os.Exit(1)
	}

	blockedAds, err := LoadBlockedAds("ads.txt")
	if err != nil {
		fmt.Println("Failed to load ads.txt:", err)
		os.Exit(1)
	}
	logChan := make(chan tea.Msg, 100)

	if *pushPort != "" {
		pushEnabled = true
		pushEndpoint = fmt.Sprintf("http://127.0.0.1:%s/log", *pushPort)
	}

	p := tea.NewProgram(model{
		cfg:       cfg,
		domains:   domains,
		blockedAds: blockedAds,
		logChan:   logChan})
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
