const HOST = process.env.HOST || 'localhost';
const PORT = process.env.PORT || 3030;

var app = require('express')();
var http = require('http').Server(app);
var io = require('socket.io')(http);

var GameRoom = require('./room');

app.get('/', function(req, res) {
   res.sendfile('index.html');
});

users = [];
rooms = {};

io.on('connection', function(socket) {
    console.log('A user connected ' + (socket.client.id));
    // INIT PLAYER
    socket.on('setPlayername', function(data) {
        if (data) {
            var isDuplicateName = false;
            for (let i = 0; i < users.length; i++) {
                const u = users[i];
                if (u.playerName == data.playerName) {
                    isDuplicateName = true;    
                    break;
                }
            }
            if(isDuplicateName) {
                socket.emit('msgError', { 
                    msg: data.playerName  + ' username is taken! Try some other username.'
                });
            } else {
                if (data.playerName.length < 5) {
                    socket.emit('msgError', { 
                        msg: data.playerName  + ' username must longer than 5 character'
                    });
                } else {
                    socket.player = data;
                    users.push(data);
                    socket.emit('playerNameSet', { 
                        id: socket.client.id,
                        name: data.playerName 
                    });
                }
            }
        }
    });
    socket.on('beep', function(data) {
       console.log('beep..');
    })
    // INIT ROOM
    socket.on('getRoomsStatus', function() {
        var results = [];
        for (let i = 0; i < 10; i++) {
            const roomName = 'room-' + (i + 1);
            results.push ({
                roomName: roomName,
                players: typeof (rooms [roomName]) !== 'undefined' 
                    ? rooms [roomName].length()
                    : 0
            });
        }
        socket.emit('updateRoomStatus', {
            rooms: results
        });
    });
    socket.on('joinOrCreateRoom', function(playerJoin) {
        if(playerJoin && socket.player) {
            var roomName = playerJoin.roomName;
            if (typeof(rooms [roomName]) === 'undefined') {
                rooms [roomName] = new GameRoom();
            }
            rooms [roomName].roomName = roomName;
            if (rooms [roomName].contain (socket) == false 
                && rooms [roomName].length() < 2) {
                rooms [roomName].join (socket);
                rooms [roomName].emitAll('newJoinRoom', {
                    roomInfo: rooms [roomName].getInfo()
                });
                socket.room = rooms [roomName];
                console.log ("A player join room. " + roomName + " Room: " + rooms [roomName].length());
            } else {
                socket.emit('joinRoomFailed', {
                    msg: rooms [roomName].contain (socket) 
                        ? "You are already join room."
                        : rooms [roomName].length() >= 2 
                            ? "Room is full."
                            : "Error, please constact with developer."
                });
            }
        }
    });
    socket.on('sendChessPosition', function(msg) {
        if(msg && socket.player && socket.room) {
            if (socket.room.length() > 1) {
                var currentPos = {
                    x: msg.posX,    // parseInt
                    y: msg.posY     // parseInt
                }
                var gameCurrentTurn = socket.room.chessLists.length % 2;
                var sendChecking = socket.game.turnIndex == msg.turnIndex 
                                && socket.game.turnIndex == gameCurrentTurn;
                // console.log (socket.game.turnIndex +" / "+ msg.turnIndex + " / " + gameCurrentTurn);
                // CAN NOT USE INDEXOF HERE  
                for (let i = 0; i < socket.room.chessLists.length; i++) {
                    const chess = socket.room.chessLists[i];
                    if (currentPos.x == chess.x && currentPos.y == chess.y) {
                        sendChecking = false;
                        break;
                    }
                }
                if (sendChecking) {
                    socket.room.emitAll('receiveChessPosition', {
                        user: socket.player.playerName,
                        currentPos,
                        turnIndex: socket.game.turnIndex
                    });
                    socket.room.chessLists.push(currentPos);
                } else {
                    socket.emit('receiveChessFail', {
                        msg: msg.turnIndex != gameCurrentTurn 
                            ? "This is NOT your turn."
                            : "You can NOT do that."
                    });
                }
            }
        }
    });
    socket.on('sendRoomChat', function(msg) {
        if(msg && socket.room) {
            socket.room.emitAll('msgChatRoom', {
                user: socket.player.playerName,
                message: msg.message
            });
        }
    });
    socket.on('leaveRoom', function() {
        if(socket.room) {
            var roomName = socket.room.roomName;
            socket.room.clearRoom();
            socket.room = null;
            delete rooms [roomName];
        }
    });
    // DISCONNECT
    socket.on('disconnect', function() {
        if (typeof(socket.player) !== 'undefined') {
            console.log ('User disconnect...' + socket.player);
            for (let i = 0; i < users.length; i++) {
                const u = users[i];
                if (u.playerName == socket.player.playerName) {
                    users.splice(i, 1);  
                    break;
                }
            }
        }
    })
});

http.listen(PORT, function() {
   console.log('listening on ' + HOST ':' + PORT);
});