mergeInto(LibraryManager.library, {
    GetDeviceType: function() {
        var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
        return isMobile ? 1 : 0;
    },

    GetURLParameters: function() {
        var params = new URLSearchParams(window.location.search);
        var matchId = params.get("matchId");
        var playerId = params.get("playerId");
        var opponentId = params.get("opponentId");
        var region = params.get("region") || "in";

        var jsonString = JSON.stringify({
            matchId: matchId,
            playerId: playerId,
            opponentId: opponentId,
            region: region
        });

        var buffer = _malloc(lengthBytesUTF8(jsonString) + 1);
        stringToUTF8(jsonString, buffer, lengthBytesUTF8(jsonString) + 1);
        return buffer;
    },

    SendMatchResult: function(matchId, playerId, opponentId, outcome, score, opponentScore, averagePing, region) {
        var matchIdStr = UTF8ToString(matchId);
        var playerIdStr = UTF8ToString(playerId);
        var opponentIdStr = UTF8ToString(opponentId);
        var outcomeStr = UTF8ToString(outcome);
        var regionStr = UTF8ToString(region);
        
        var result = {
            matchId: matchIdStr,
            playerId: playerIdStr,
            opponentId: opponentIdStr,
            outcome: outcomeStr,
            score: score,
            opponentScore: opponentScore,
            averagePing: averagePing,
            region: regionStr
        };
        
        console.log("[WebGL] SendMatchResult:", result);
        
        // Send to parent window if in iframe
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'match_result',
                payload: {
                    matchId: matchIdStr,
                    playerId: playerIdStr,
                    opponentId: opponentIdStr,
                    outcome: outcomeStr,
                    score: score,
                    opponentScore: opponentScore,
                    averagePing: averagePing,
                    region: regionStr
                }
            }, '*');
        }
    },

    SendMatchAbort: function(message, error, errorCode) {
        var messageStr = UTF8ToString(message);
        var errorStr = UTF8ToString(error);
        var errorCodeStr = UTF8ToString(errorCode);
        
        var abortData = {
            message: messageStr,
            error: errorStr,
            errorCode: errorCodeStr
        };
        
        console.log("[WebGL] SendMatchAbort:", abortData);
        
        // Send to parent window if in iframe
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'match_abort',
                payload: {
                    message: messageStr,
                    error: errorStr,
                    errorCode: errorCodeStr
                }
            }, '*');
        }
    },

    SendScreenshot: function(base64Ptr) {
        var base64 = UTF8ToString(base64Ptr);
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'game_state',
                payload: {
                    state: base64
                }
            }, '*');
        }
    },

    SendGameState: function(statePtr) {
        var state = UTF8ToString(statePtr);
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'game_state',
                payload: {
                    state: state
                }
            }, '*');
        }
    },

    SendBuildVersion: function(versionPtr) {
        var version = UTF8ToString(versionPtr);
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'build_version',
                payload: {
                    version: version
                }
            }, '*');
        }
    },

    SendGameReady: function() {
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'game_ready',
                payload: {}
            }, '*');
        }
    },

    IsMobileWeb: function() {
        var isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
        console.log("[WebGL] IsMobileWeb:", isMobile);
        return isMobile ? 1 : 0;
    }
});
