# WebSocketMITM
MITM server for WebSocket protocol via DNS poisoning

# How to use?

See example in https://github.com/AsenOsen/WebSocketMITM/blob/main/Program.cs

# NGINX Splitter

In cases when HTTP and WebSocket traffic use the same port (Upgrade), use NGINX with protocol splitting config - https://github.com/AsenOsen/WebSocketMITM/blob/main/nginx.conf
