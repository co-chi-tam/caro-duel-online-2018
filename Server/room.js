
function GameRoom() {
    // Current room name.
    this.roomName = '';
    // All players 
    this.players = [];
    // Turn index X or O
    this.turns = [0, 1];    // 'BLUE', 'RED'
    // LOGIC GAME
    // Remain all chess actived
    this.chessLists = [];

    // Get current turn.
    this.currentTurn = function() {
        return this.chessLists.length % 2; // RED or BLUE
    }

    // Clear list chess actived
    this.clearChess = function() {
        this.chessLists = [];
    }

    // Join room and set turn index for player
    this.join = function (player) {
        if (this.players.indexOf (player) == -1) {
            var self = this;
            this.players.push (player);
            for (let i = 0; i < this.players.length; i++) {
                const ply = this.players[i];
                const turn = this.turns [i % 2];
                ply.game = {
                    turnIndex: turn // RED or BLUE
                };
                ply.emit('turnIndexSet', {
                    turnIndex: turn
                });
            }
        }
    };

    // Clear room
    this.clearRoom = function() {
        for (let i = 0; i < this.players.length; i++) {
            const ply = this.players[i];
            ply.emit('clearRoom', {
                msg: "Room is empty or player is quit."
            });
        }
        this.players = [];
        this.clearChess();
    };
    
    // Leave room 
    this.leave = function(player) {
        if (this.players.indexOf (player) > -1) {
            this.players.splice (this.players.indexOf (player), 1);
            console.log ('User LEAVE ROOM...' + player.player);
        }
    };
    
    // Send all mesg for players in room.
    this.emitAll  = function (name, obj) {
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            player.emit(name, obj);
        }
    };

    // Get rom info.
    this.getInfo = function() {
        var playerInfoes = [];
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            playerInfoes.push (player.player);
        }
        return {
            roomName: this.roomName,
            players: playerInfoes
        };
    }

    // If client contain in room.
    this.contain = function (player) {
        return this.players.indexOf (player) > -1;
    };
    
    // Get amount of players in room.
    this.length  = function () {
        return this.players.length;
    };
};
// INIT
module.exports = GameRoom;