    default rel
    section .text
    global main
main:
    ; begin while loop at line yo 6
lbl2:
    push qword 1
    pop rax
    cmp rax, 0
    je lbl1
    push qword 0
    pop rax
    cmp rax,0
    je lbl0
    push qword 1
    pop rax
    ret
lbl0:
    jmp lbl2
lbl1:
    section .data
