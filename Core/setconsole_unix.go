/*
 **********************************************************************
 * -------------------------------------------------------------------
 * Project Name : Abdal 4iProto Client
 * File Name    : setconsole_unix.go
 * Author       : Ebrahim Shafiei (EbraSha)
 * Email        : Prof.Shafiei@Gmail.com
 * Created On   : 2025-01-27 10:30:00
 * Description  : Unix/Linux-specific implementation for setting console title
 * -------------------------------------------------------------------
 *
 * "Coding is an engaging and beloved hobby for me. I passionately and insatiably pursue knowledge in cybersecurity and programming."
 * – Ebrahim Shafiei
 *
 **********************************************************************
 */

//go:build !windows

package main

import (
	"fmt"
)

// SetConsoleTitle sets the terminal window title on Unix/Linux using ANSI escape sequence
func SetConsoleTitle(title string) {
	fmt.Printf("\033]0;%s\007", title)
}

