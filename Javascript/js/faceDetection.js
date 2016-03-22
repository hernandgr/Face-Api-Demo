$('#btn').click(function () {
    var file = document.getElementById('filename').files[0];
    detectFaces(file);
});

$("#filename").change(function () {
    showImage();
});

function detectFaces(file) {
    var apiKey = "Your key here :)";
    
    // Call the API
    $.ajax({
        url: "https://api.projectoxford.ai/face/v1.0/detect",
        beforeSend: function (xhrObj) {
            xhrObj.setRequestHeader("Content-Type", "application/octet-stream");
            xhrObj.setRequestHeader("Ocp-Apim-Subscription-Key", apiKey);
            $("#response").text("Calling api...");
        },
        type: "POST",
        data: file,
        processData: false
    })
        .done(function (response) {
            // Process the API response.
            processResult(response);
        })
        .fail(function (error) {
            // Oops, an error :(
            $("#response").text(error.getAllResponseHeaders());
        });
}

function processResult(response) {
    var arrayLength = response.length;

    if (arrayLength > 0) {
        var canvas = document.getElementById('myCanvas');
        var context = canvas.getContext('2d');

        context.beginPath();
        
        // Draw face rectangles into canvas.
        for (var i = 0; i < arrayLength; i++) {
            var faceRectangle = response[i].faceRectangle;
            context.rect(faceRectangle.left, faceRectangle.top, faceRectangle.width, faceRectangle.height);
        }

        context.lineWidth = 3;
        context.strokeStyle = 'red';
        context.stroke();
    }

    // Show the raw response.
    var data = JSON.stringify(response);
    $("#response").text(data);
}

function showImage() {
    var canvas = document.getElementById("myCanvas");
    var context = canvas.getContext("2d");
    context.clearRect(0, 0, canvas.width, canvas.height);

    var input = document.getElementById("filename");
    var img = new Image;

    img.onload = function () {
        context.drawImage(img, 0, 0);
    }

    img.src = URL.createObjectURL(input.files[0]);
}