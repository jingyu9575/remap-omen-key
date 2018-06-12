@gcc -std=c99 -Wall -Wextra -nostdlib -ffreestanding -mwindows -O2 -fno-stack-check -fno-stack-protector -mno-stack-arg-probe -o send-home.exe send-home.c -luser32 -lkernel32
