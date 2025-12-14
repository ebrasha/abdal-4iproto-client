/*
 **********************************************************************
 * -------------------------------------------------------------------
 * Project Name : Abdal 4iProto Client
 * File Name    : ip_defrag.go
 * Author       : Ebrahim Shafiei (EbraSha)
 * Email        : Prof.Shafiei@Gmail.com
 * Created On   : 2025-12-14 15:17:47
 * Description  : IP packet defragmentation module using gopacket for handling fragmented UDP packets in SOCKS5 proxy
 * -------------------------------------------------------------------
 *
 * "Coding is an engaging and beloved hobby for me. I passionately and insatiably pursue knowledge in cybersecurity and programming."
 * – Ebrahim Shafiei
 *
 **********************************************************************
 */

package main

import (
	"fmt"
	"net"
	"os"
	"runtime"
	"sync"
	"syscall"

	"github.com/google/gopacket"
	"github.com/google/gopacket/ip4defrag"
	"github.com/google/gopacket/layers"
	"golang.org/x/net/ipv4"
)

// IP_HDRINCL constant for Windows (value is 2)
const IP_HDRINCL = 2

// IPDefragmenter handles IP packet defragmentation for UDP packets
type IPDefragmenter struct {
	defragger *ip4defrag.IPv4Defragmenter
	mu        sync.Mutex
}

// NewIPDefragmenter creates a new IP defragmenter instance
func NewIPDefragmenter() *IPDefragmenter {
	return &IPDefragmenter{
		defragger: ip4defrag.NewIPv4Defragmenter(),
	}
}

// DefragmentIPPacket processes an IP packet and returns the defragmented packet if complete
func (d *IPDefragmenter) DefragmentIPPacket(ipPacket []byte) ([]byte, error) {
	d.mu.Lock()
	defer d.mu.Unlock()

	// Parse the IP packet
	packet := gopacket.NewPacket(ipPacket, layers.LayerTypeIPv4, gopacket.Default)
	ipLayer := packet.Layer(layers.LayerTypeIPv4)
	if ipLayer == nil {
		return nil, fmt.Errorf("not an IPv4 packet")
	}

	ip, _ := ipLayer.(*layers.IPv4)

	// Check if packet is fragmented
	isFragmented := (ip.Flags&layers.IPv4MoreFragments != 0) || (ip.FragOffset > 0)
	if !isFragmented {
		// Not fragmented, return as is
		return ipPacket, nil
	}

	// Defragment the packet
	defragged, err := d.defragger.DefragIPv4(ip)
	if err != nil {
		return nil, fmt.Errorf("defragmentation error: %w", err)
	}

	if defragged == nil {
		// Packet is still being assembled, return nil to indicate incomplete
		return nil, nil
	}

	// Serialize the defragmented packet
	buf := gopacket.NewSerializeBuffer()
	opts := gopacket.SerializeOptions{
		FixLengths:       true,
		ComputeChecksums: true,
	}
	err = defragged.SerializeTo(buf, opts)
	if err != nil {
		return nil, fmt.Errorf("failed to serialize defragmented packet: %w", err)
	}

	return buf.Bytes(), nil
}

// UDPPacketReader reads UDP packets from a raw socket with IP defragmentation support
type UDPPacketReader struct {
	defragger    *IPDefragmenter
	rawConn      *ipv4.PacketConn
	udpConn      *net.UDPConn
	useRaw       bool
	localUDPPort int
}

// NewUDPPacketReader creates a new UDP packet reader with defragmentation support
// Attempts to use raw socket for IP-level packet capture, falls back to regular UDP if unavailable
// Note: Raw sockets require administrator privileges on Windows
func NewUDPPacketReader(udpConn *net.UDPConn) (*UDPPacketReader, error) {
	reader := &UDPPacketReader{
		defragger:    NewIPDefragmenter(),
		udpConn:      udpConn,
		useRaw:       false,
		localUDPPort: udpConn.LocalAddr().(*net.UDPAddr).Port,
	}

	// Try to create a raw socket for IP-level packet capture
	// This requires administrator privileges on Windows
	rawSocket, err := syscall.Socket(syscall.AF_INET, syscall.SOCK_RAW, syscall.IPPROTO_IP)
	if err != nil {
		// Raw socket not available (likely no admin privileges), fall back to regular UDP
		// OS will handle defragmentation, but we can't intercept fragmented packets
		return reader, nil
	}

	// Set socket options to receive IP headers
	// Use platform-specific constant for IP_HDRINCL
	var ipHdrIncl int
	if runtime.GOOS == "windows" {
		ipHdrIncl = IP_HDRINCL
	} else {
		// On Unix systems, try to use syscall.IP_HDRINCL if available
		// If not available, we'll skip this option
		ipHdrIncl = 1 // Try a generic value, may not work on all systems
	}

	err = syscall.SetsockoptInt(rawSocket, syscall.IPPROTO_IP, ipHdrIncl, 1)
	if err != nil {
		syscall.Close(rawSocket)
		return reader, nil
	}

	// Create a file from the socket descriptor
	file := os.NewFile(uintptr(rawSocket), "raw-socket")
	if file == nil {
		syscall.Close(rawSocket)
		return reader, nil
	}

	// Create a packet connection from the file (not regular connection)
	packetConn, err := net.FilePacketConn(file)
	file.Close() // Close the file, we have the connection now
	if err != nil {
		syscall.Close(rawSocket)
		return reader, nil
	}

	// Create ipv4.PacketConn from the packet connection
	rawConn := ipv4.NewPacketConn(packetConn)
	if rawConn == nil {
		packetConn.Close()
		return reader, nil
	}

	reader.rawConn = rawConn
	reader.useRaw = true
	return reader, nil
}

// ReadUDPPacket reads a complete UDP packet, handling fragmentation if needed
func (r *UDPPacketReader) ReadUDPPacket() ([]byte, *net.UDPAddr, error) {
	if r.useRaw && r.rawConn != nil {
		// Use raw socket with defragmentation
		buf := make([]byte, 65535)
		for {
			n, cm, src, err := r.rawConn.ReadFrom(buf)
			if err != nil {
				return nil, nil, err
			}

			// Extract source IP from control message or address
			var srcIP net.IP
			if cm != nil && cm.Src != nil {
				// cm.Src is net.IP (which is []byte)
				srcIP = cm.Src
			} else if src != nil {
				if ipAddr, ok := src.(*net.IPAddr); ok {
					srcIP = ipAddr.IP
				} else if udpAddr, ok := src.(*net.UDPAddr); ok {
					srcIP = udpAddr.IP
				}
			}

			// Parse IP packet
			packet := gopacket.NewPacket(buf[:n], layers.LayerTypeIPv4, gopacket.Default)
			ipLayer := packet.Layer(layers.LayerTypeIPv4)
			if ipLayer == nil {
				continue
			}

			ip, _ := ipLayer.(*layers.IPv4)

			// Use source IP from packet if not available from control message
			if srcIP == nil {
				srcIP = ip.SrcIP
			}

			// Check if packet is fragmented
			isFragmented := (ip.Flags&layers.IPv4MoreFragments != 0) || (ip.FragOffset > 0)
			if isFragmented {
				// Defragment the packet
				defragged, err := r.defragger.DefragmentIPPacket(buf[:n])
				if err != nil {
					// Error during defragmentation, skip this packet
					continue
				}
				if defragged == nil {
					// Still assembling, skip this packet and wait for more fragments
					continue
				}

				// Re-parse the defragmented packet
				packet = gopacket.NewPacket(defragged, layers.LayerTypeIPv4, gopacket.Default)
				ipLayer = packet.Layer(layers.LayerTypeIPv4)
				if ipLayer == nil {
					continue
				}
				ip, _ = ipLayer.(*layers.IPv4)
			}

			// Extract UDP layer
			udpLayer := packet.Layer(layers.LayerTypeUDP)
			if udpLayer == nil {
				continue
			}

			udp, _ := udpLayer.(*layers.UDP)

			// Check if this UDP packet is for our port
			if int(udp.DstPort) != r.localUDPPort {
				continue
			}

			// Extract source address
			clientAddr := &net.UDPAddr{
				IP:   srcIP,
				Port: int(udp.SrcPort),
			}

			// Return the UDP payload (SOCKS5 packet)
			return udp.Payload, clientAddr, nil
		}
	}

	// Fall back to regular UDP socket reading
	// The OS should handle defragmentation, but we'll use larger buffer
	buf := make([]byte, 65535)
	n, clientAddr, err := r.udpConn.ReadFromUDP(buf)
	if err != nil {
		return nil, nil, err
	}
	return buf[:n], clientAddr, nil
}

// Close closes the packet reader
func (r *UDPPacketReader) Close() error {
	if r.rawConn != nil {
		r.rawConn.Close()
	}
	return nil
}
