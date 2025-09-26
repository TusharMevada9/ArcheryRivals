mergeInto(LibraryManager.library, {
    GetDeviceType: function() {
        var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
        return isMobile ? 1 : 0;
    },

    GetURLParameters: function() {
        try {
            var params = new URLSearchParams(window.location.search);
            var matchId = params.get("matchId") || "";
            var playerId = params.get("playerId") || "";
            var opponentId = params.get("opponentId") || "";
            var region = params.get("region") || "in";

            var jsonString = JSON.stringify({
                matchId: matchId,
                playerId: playerId,
                opponentId: opponentId,
                region: region
            });

            var bufferSize = lengthBytesUTF8(jsonString) + 1;
            var buffer = _malloc(bufferSize);
            if (buffer === 0) {
                console.error("[WebGL] Failed to allocate memory for GetURLParameters");
                return 0;
            }
            
            stringToUTF8(jsonString, buffer, bufferSize);
            return buffer;
        } catch (e) {
            console.error("[WebGL] Error in GetURLParameters:", e);
            return 0;
        }
    },

    SendMatchResult: function(matchId, playerId, opponentId, outcome, score, opponentScore, averagePing, region) {
        try {
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
        } catch (e) {
            console.error("[WebGL] Error in SendMatchResult:", e);
        }
    },

    SendMatchAbort: function(messagePtr, errorPtr, errorCodePtr) {
        try {
            var message = UTF8ToString(messagePtr);
            var error = UTF8ToString(errorPtr);
            var errorCode = UTF8ToString(errorCodePtr);
            
            var abortData = {
                message: message,
                error: error,
                errorCode: errorCode
            };
            
            console.log("[WebGL] SendMatchAbort:", abortData);
            
            // Send to parent window if in iframe
            if (window.parent && window.parent !== window) {
                window.parent.postMessage({
                    type: 'match_abort',
                    payload: {
                        message: message,
                        error: error,
                        errorCode: errorCode
                    }
                }, '*');
            }
        } catch (e) {
            console.error("[WebGL] Error in SendMatchAbort:", e);
        }
    },

    IsMobileWeb: function() {
        var isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
        console.log("[WebGL] IsMobileWeb:", isMobile);
        return isMobile ? 1 : 0;
    }
});
