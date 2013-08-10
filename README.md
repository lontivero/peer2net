Peer2Net
======

Peer2Net is a simple framework to develop peer-to-peer applications in .NET and Mono. 


Goals
-----
Simplicity. Peer2Net abstracts away many of the existing complexities we find when develop with sockets and provides a clean and easy interface to send and receive data to/from peers.
Avoid multi-threading. Peer2Net uses a single thread approach to handle all the asynchronous operations and queue the communications' results in a independent queue for processing. Developers just have to take the queue's elements and that´s all. 
Low-memory usage.  Peer2Net requires less than 4 MB to do its job. Internally, pooled objects and pre-allocated buffers are used to reduce the memory requirements.
Low-grained bandwidth control. Peer2Net allows to control the individual peer's upload and download speed.


Development
-----------
Peer2Net is developed by [Lucas Ontivero](http://geeks.ms/blogs/lontivero) ([@lontivero](http://twitter.com/lontivero)). You are welcome to contribute code. You can send code both as a patch or a GitHub pull request.

Note that Peer2Net is still very much work in progress. 