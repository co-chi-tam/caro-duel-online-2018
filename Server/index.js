// HOST 
const HOST = process.env.HOST || 'localhost';
// PORT
const PORT = process.env.PORT || 3030;

var express = require('express');
var app = express();
var http = require('http').Server(app);
// GAME LOGIC XO
var GameXO = require('./gamexo')(http);

// SEND index.html page
app.get('/', function(req, res) {
   res.sendfile('index.html');
});

// SERVER LISTEN WITH PORT
http.listen(PORT, function() {
   console.log('listening on ' + HOST + ':' + PORT);
}); 