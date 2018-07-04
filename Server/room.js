
function GameRoom() {
    this.roomName = '';
    this.players = [];
    this.turns = [0, 1];    // 'BLUE', 'RED'
    // LOGIC GAME
    this.chessLists = [];

    this.currentTurn = function() {
        return this.chessLists.length % 2; // RED or BLUE
    }

    this.clearChess = function() {
        this.chessLists = [];
    }

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
            // LEAVE ROOM
            player.on('disconnect', function() {
                self.clearRoom();
            });
        }
    };

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
    
    this.leave = function(player) {
        if (this.players.indexOf (player) > -1) {
            this.players.splice (this.players.indexOf (player), 1);
            console.log ('User LEAVE ROOM...' + player.player);
        }
    };
    
    this.emitAll  = function (name, obj) {
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            player.emit(name, obj);
        }
    };

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

    this.contain = function (player) {
        return this.players.indexOf (player) > -1;
    };
    
    this.length  = function () {
        return this.players.length;
    };
};

module.exports = GameRoom;