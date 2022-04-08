const express = require("express");
const path = require('path');
const app = express();

app.set("view options", {layout: false});
app.use(express.static(__dirname + '/public'));

//#region old school
const bodyContents = `<center><h2><BR><font face="Comic Sans MS" color = #5555aa>Hello WebSockets!</font></h2>
<p><a href="http://localhost:8000/player/spark/1/2/3"><font color = #00eeff>example</font></a></p></center>`;
const bgImage = "https://live.staticflickr.com/1863/29641403557_f5b238b603_o.jpg";
//#endregion

app.get("/", (req, res) => {
    //accessed via webrowser
    res.setHeader('Content-type','text/html')
    res.send(`<html><body bgcolor =#000000 background="${bgImage}">${bodyContents}</body></html>`);
    //res.sendFile(path.join(__dirname, '/Welcome.html')); //uncomment to send properly formatted file instead
});

//example to send params http://localhost:8000/player/spark/1/2/3
app.get("/player/:id/:x/:y/:z", (req, res) => {
    // Return the user's info 
    var playerData = {
        "playerID": req.params["id"],
        "playerName": "muammar",
        "pos": [
            { name: "X", value: req.params["x"] },
            { name: "Y", value: req.params["y"] },
            { name: "Z", value: req.params["z"] }
        ]
    };
    res.json(playerData);
});

app.listen( 8000, () => {
    console.log("Server started!");
} );

