# BruteForceTool

Tool to brute force login page on DVWA.

## Useage

Example useage:

```bat
BruteForceTool.exe -a "http://127.0.0.1:4280/vulnerabilities/brute/?username=admin&password={PASSWORD}&Login=Login" -f C:\Users\mende\Documents\Github\wordlists\wordlists\passwords\most_used_passwords.txt -c "PHPSESSID=d49e698cba50cd9fbb379e4a96158e96; security=low"
```
