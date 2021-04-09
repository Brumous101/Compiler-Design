    default rel
    section .text
    global main
main:
    push qword 1
    pop rax
    mov [$lbl0],rax
    push qword 2
    pop rax
    mov [$lbl1],rax
    mov rax, __float64__(3.25)
    push rax
    mov rax, [$lbl1]
    push qword rax
    pop rax
    cvtsi2sd xmm0, rax
    sub rsp,8
    movq [rsp], xmm0
    movq xmm1, [rsp]
    add rsp,8
    movq xmm0, [rsp]
    add rsp,8
    addsd xmm0,xmm1
    sub rsp,8
    movq [rsp], xmm0
    mov rax, __float64__(0.25)
    push rax
    movq xmm1, [rsp]
    add rsp,8
    movq xmm0, [rsp]
    add rsp,8
    addsd xmm0,xmm1
    sub rsp,8
    movq [rsp], xmm0
    pop rax
    mov [$lbl2],rax
    mov rax, __float64__(4.0)
    push rax
    pop rax
    mov [$lbl3],rax
    mov rax, [$lbl2]
    push qword rax
    movq xmm0, [rsp]
    add rsp,8
    cvttsd2si rax, xmm0
    push rax
    pop rax
    ret
    section .data
lbl0:
    dq 0
lbl1:
    dq 0
lbl2:
    dq 0
lbl3:
    dq 0
