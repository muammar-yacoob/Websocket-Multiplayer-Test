
const http = require("http");

const host = 'localhost';
const port = 8000;
var visitCount = 0;
const requestListener = function (req, res)
 {
    res.setHeader("content-Type","text/plain");
    res.write(`Hello HTTP! ${visitCount++}`); //write a response to the client i.e. web browser (http://localhost:8000/)
    res.end(); //end the response
 };

const server = http.createServer(requestListener);
server.listen(port, host, () => {
    console.log(`Server is running on http://${host}:${port}`);//logging to terminal
});