mergeInto(LibraryManager.library, {
    GetURLParameters: function() {
        var urlParams = new URLSearchParams(window.location.search);
        var params = {};
        for (var pair of urlParams.entries()) {
            params[pair[0]] = pair[1];
        }
        var jsonString = JSON.stringify(params);
        var bufferSize = lengthBytesUTF8(jsonString) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(jsonString, buffer, bufferSize);
        return buffer;
    },

    SendMatchResult: function(matchId, playerId, opponentId, outcome, score, opponentScore) {
        var matchIdStr = UTF8ToString(matchId);
        var playerIdStr = UTF8ToString(playerId);
        var opponentIdStr = UTF8ToString(opponentId);
        var outcomeStr = UTF8ToString(outcome);
        
        var result = {
            matchId: matchIdStr,
            playerId: playerIdStr,
            opponentId: opponentIdStr,
            outcome: outcomeStr,
            score: score,
            opponentScore: opponentScore
        };
        
        console.log("[WebGL] SendMatchResult:", result);
        
        // Send to parent window if in iframe
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'matchResult',
                data: result
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
                type: 'matchAbort',
                data: abortData
            }, '*');
        }
    },

    SendGameState: function(state) {
        var stateStr = UTF8ToString(state);
        
        console.log("[WebGL] SendGameState:", stateStr);
        
        // Send to parent window if in iframe
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'gameState',
                data: stateStr
            }, '*');
        }
    },

    SendBuildVersion: function(version) {
        var versionStr = UTF8ToString(version);
        
        console.log("[WebGL] SendBuildVersion:", versionStr);
        
        // Send to parent window if in iframe
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'buildVersion',
                data: versionStr
            }, '*');
        }
    },

    SendGameReady: function() {
        console.log("[WebGL] SendGameReady");
        
        // Send to parent window if in iframe
        if (window.parent && window.parent !== window) {
            window.parent.postMessage({
                type: 'gameReady',
                data: 'ready'
            }, '*');
        }
    },

    IsMobileWeb: function() {
        var isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
        console.log("[WebGL] IsMobileWeb:", isMobile);
        return isMobile ? 1 : 0;
    }
});
