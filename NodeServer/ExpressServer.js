const express = require("express");
const path = require('path');
const app = express();
const host = 'localhost';
const port = process.env.port || 8000;

app.set("view options", {layout: false});
app.use(express.static(__dirname + '/public'));

//#region Webpage
//#region old school
const jsonExample = `http://localhost:8000/unity/{"playerID":15078,"playerName":"BlueSphere","pos":{"x":0,"y":1.85,"z":0}}`;
const bodyContents = `<center><h1><BR><font face="Comic Sans MS" color = #5555aa>Hello WebSockets!!</font></h1>
<p><a href="${jsonExample}"><font color = #00eeff>example</font></a></p></center>`;
const bgImage = "https://live.staticflickr.com/1863/29641403557_f5b238b603_o.jpg?width=*";
//#endregion

app.get("/", (req, res) => {
    //accessed via webrowser
    console.log("received web");

    res.setHeader('Content-type','text/html')
    res.send(`<html><body bgcolor =#000000 background="${bgImage}">${bodyContents}</body></html>`);
    //res.sendFile(path.join(__dirname, '/Welcome.html')); //uncomment to send properly formatted file instead
});
//#endregion

//Accessed From Unity
app.use(express.json());
app.get("/unity/", (req, res) => {
    res.setHeader('Content-Type', 'application/json');
    //console.log(`received json: ${req.body}`);
    res.send(req.body);//echo result back
});

app.listen(port, host, () => {
    console.log(`Server is running on http://${host}:${port}`);//logging to terminal
});

