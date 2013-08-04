P2PNet
======

P2PNet is a simple framework to develop peer-to-peer applications in .NET and Mono. 


Goals
-----
Simplicity. P2PNet abstracts away many of the existing complexities we find when develop with sockets and provides a clean and easy interface to send and receive data to/from peers.
Avoid multi-threading. P2PNet uses a single thread approach to handle all the asynchronous operations and enqueue the communications' results in a independent queue for processing. Developers just have to take the queue's elements and that´s all. 
Low-memory usage.  P2PNet requires less than 4 MB to do its job. Internally, pooled objects and pre-allocated buffers are used to reduce the memory requirements.
Low-grained bandwidth control. P2PNet allows to control the individual peer's upload and download speed.


Development
-----------
P2PNet is developed by [Lucas Ontivero](http://geeks.ms/blogs/lontivero) ([@lontivero](http://twitter.com/lontivero)). You are welcome to contribute code. You can send code both as a patch or a GitHub pull request.

Note that P2PNet is still very much work in progress. 