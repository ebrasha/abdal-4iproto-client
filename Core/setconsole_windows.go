/*
 **********************************************************************
 * -------------------------------------------------------------------
 * Project Name : Abdal 4iProto Client
 * File Name    : setconsole_windows.go
 * Author       : Ebrahim Shafiei (EbraSha)
 * Email        : Prof.Shafiei@Gmail.com
 * Created On   : 2025-01-27 10:30:00
 * Description  : Windows-specific implementation for setting console title
 * -------------------------------------------------------------------
 *
 * "Coding is an engaging and beloved hobby for me. I passionately and insatiably pursue knowledge in cybersecurity and programming."
 * – Ebrahim Shafiei
 *
 **********************************************************************
 */

//go:build windows

package main

import (
	"syscall"
	"unsafe"
)

// SetConsoleTitle sets the terminal window title on Windows using Windows API
func SetConsoleTitle(title string) {
	ptr := syscall.StringToUTF16Ptr(title)
	kernel32 := syscall.NewLazyDLL("kernel32.dll")
	setConsoleTitle := kernel32.NewProc("SetConsoleTitleW")
	setConsoleTitle.Call(uintptr(unsafe.Pointer(ptr)))
}

