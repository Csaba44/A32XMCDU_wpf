; Control + space

^SPACE::
WinGet, hwnd, ID, A
WinGet, style, Style, ahk_id %hwnd%
if (style & 0x00C00000) {
    WinSet, Style, -0x00C40000, ahk_id %hwnd%
    WinMaximize, ahk_id %hwnd%
} else {
    WinSet, Style, +0x00C40000, ahk_id %hwnd%
    WinRestore, ahk_id %hwnd%
}
return