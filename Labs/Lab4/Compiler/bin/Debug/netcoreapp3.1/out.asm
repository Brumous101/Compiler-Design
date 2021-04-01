    default rel
    section .text
    global main
main:
    ; begin while loop at line yo 4
lbl2:
    mov rax, 1
    cmp rax, 0
    je lbl1
    mov rax, 0
    cmp rax,0
    je lbl0
    mov rax, 1
    ret
lbl0:
    jmp lbl2
lbl1:
    section .data
