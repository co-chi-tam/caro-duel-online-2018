
var GameRoom = require('./room'); // ROOM LOGIC

users = []; // Array User names
rooms = {}; // Rooms
column = 7; 
row = 8;

const MAXIMUM_ROOMS = 10; // Maximum rooms
const MAXIMUM_PLAYERS = 2; // MAXIMUM PLAYERS

var GameXO = function (http) {
    var io = require('socket.io')(http); // Require socket.io
    
    joinRoom = function(socket, roomName)
    {
        if(roomName && socket.player) {
            if (typeof(rooms [roomName]) === 'undefined') {
                rooms [roomName] = new GameRoom();
            }
            rooms [roomName].roomName = roomName;
            if (rooms [roomName].contain (socket) == false) {
                if (rooms [roomName].length() < 2) {
                    rooms [roomName].join (socket);
                    socket.room = rooms [roomName];
                    socket.player.chessLists = [];
                    rooms [roomName].emitAll('newJoinRoom', {
                        roomInfo: rooms [roomName].getInfo()
                    });    
                    console.log ("A player join room. " + roomName + " Room: " + rooms [roomName].length());
                } else {
                    socket.emit('joinRoomFailed', {
                        msg: "Room is full. Please try again late."
                    });
                }
            } else {
                socket.emit('joinRoomFailed', {
                    msg: "You are already join room."
                });
            }
        }
    }

    // On client connect.
    io.on('connection', function(socket) {
        console.log('A user connected ' + (socket.client.id));
        // Welcome message
        socket.emit('welcome', { 
            msg: 'Welcome to connect game caro duel online.'
        });
        // INIT PLAYER
        // Set player name.
        socket.on('setPlayername', function(data) {
            if (data && data.playerName) {
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
        // Receive beep mesg
        socket.on('beep', function(data) {
        socket.emit('boop');
        })
        // INIT ROOM
        // Get all room status
        socket.on('getRoomsStatus', function() {
            var results = [];
            for (let i = 0; i < MAXIMUM_ROOMS; i++) {
                const roomName = 'room-' + (i + 1);
                const playerCount = typeof (rooms [roomName]) !== 'undefined' 
                                        ? rooms [roomName].length()
                                        : 0;
                results.push ({
                    roomName: roomName,
                    roomDisplay: '[' + roomName + ']: ' + playerCount + '/2',
                    players: playerCount
                });
            }
            socket.emit('updateRoomStatus', {
                rooms: results
            });
        });
        // Join or create room by name. 
        socket.on('joinOrCreateRoom', function(playerJoin) {
            joinRoom (socket, playerJoin.roomName);
        });
        // Join or create room by name. 
        socket.on('joinExistedRoom', function() {
            for (let i = 0; i < MAXIMUM_ROOMS; i++) {
                const roomName = 'room-' + (i + 1);
                const playerCount = typeof (rooms [roomName]) !== 'undefined' 
                                        ? rooms [roomName].length()
                                        : 0;
                if (playerCount >= 0 && playerCount < MAXIMUM_PLAYERS)
                {
                    joinRoom (socket, roomName);
                    return;
                }
            }
        });
        // Send chess position with check available.
        socket.on('sendChessPosition', function(msg) {
            if(msg && socket.player && socket.room) {
                if (socket.room.length() > 1) {
                    var currentPos = {
                        x: msg.posX,    // parseInt
                        y: msg.posY     // parseInt
                    }
                    var gameCurrentTurn = socket.room.chessLists.length % MAXIMUM_PLAYERS;
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
                        socket.player.chessLists.push(currentPos);
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
        // Send chess position without check available.
        socket.on('sendChessSpot', function(msg) {
            if(msg && socket.player && socket.room) {
                if (socket.room.length() > 1) {
                    var currentPos = {
                        x: msg.posX,    // parseInt
                        y: msg.posY     // parseInt
                    }
                    var gameCurrentTurn = socket.room.chessLists.length % MAXIMUM_PLAYERS;
                    var sendChecking = socket.game.turnIndex == msg.turnIndex 
                                    && socket.game.turnIndex == gameCurrentTurn;
                    if (sendChecking) {
                        socket.room.emitAll('receiveChessPosition', {
                            user: socket.player.playerName,
                            currentPos,
                            turnIndex: socket.game.turnIndex
                        });
                        socket.room.chessLists.push(currentPos);
                        socket.player.chessLists.push(currentPos);
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
        // Receive client chat in current room.
        socket.on('sendRoomChat', function(msg) {
            if(msg && socket.room) {
                socket.room.emitAll('msgChatRoom', {
                    user: socket.player.playerName,
                    message: msg.message
                });
            }
        });
        // Receive world chat.
        socket.on('sendWorldChat', function(msg) {
            if(msg) {
                // socket.broadcast.emit => will send the message to all the other clients except the newly created connection
                io.sockets.emit('msgWorldChat', {
                    user: socket.player.playerName,
                    message: msg.message
                });
            }
        });
        // Receive leave room mesg.
        socket.on('leaveRoom', function() {
            console.log ('User leave room...' + socket.id);
            if(socket.room) {
                var roomName = socket.room.roomName;
                socket.room.clearRoom();
                delete socket.room;
                delete rooms [roomName];
            }
        });
        // DISCONNECT
        // Disconnect and clear room.
        socket.on('disconnect', function() {
            console.log ('User disconnect...' + socket.id);
            if (socket.player) {
                for (let i = 0; i < users.length; i++) {
                    const u = users[i];
                    if (u.playerName == socket.player.playerName) {
                        users.splice(i, 1);  
                        break;
                    }
                }
            }
            // LEAVE ROOM
            if (socket.room) {
                var roomName = socket.room.roomName;
                socket.room.clearRoom();
                delete socket.room;
                delete rooms [roomName];
            }
        });
    });
    // SHUFFLE ARRAY
    this.shuffle = function(a) {
        var j, x, i;
        for (i = a.length - 1; i > 0; i--) {
            j = Math.floor(Math.random() * (i + 1));
            x = a[i];
            a[i] = a[j];
            a[j] = x;
        }
        return a;
    }
};
// INIT
module.exports = GameXO;