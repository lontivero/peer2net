Open.P2P
========

Open.P2P is a lightweight and easy-to-use class library to develop peer-to-peer applications in .NET and Mono. 
Programming with sockets, and using tcp as communication protocol in particular, introduces some complexites which can be abstracted away.


Goals
-----
The main goal is to simplify communication amoung computers abstracting away most of the complexities developers find when develop with sockets, providing a clean and easy interface to connect, disconnect, send  and receive data to/from peers. 


Example
--------
A simple Echo service that once received a message, sends it back.

```c#
var listener = new Listener(9988);
var comManager = new CommunicationManager(_listener);

comManager.PeerConnected += (s, e)=> {
    using(var sr = new StreamReader(e.Peer.Stream))
	{
		using(var sw = new StreamWriter(e.Peer.Stream))
		{
			var read = sr.Read(buf, 0, buf.Length)
			while(read > 0)
			{
				sw.Write(buf, 0, read);
				read = sr.Read(buf, 0, buf.Length);
			}
		}
	}
};
listener.Start();
```

API
------
The main functionality is available trought the CommunicationManager class and its four methods: Connect, Disconnect, Send and Receive.

![alt tag](http://geeks.ms/cfs-file.ashx/__key/CommunityServer.Blogs.Components.WeblogFiles/lontivero/CommunicationManager_5F00_4FE194D4.png)


Points of interest
-------------------

Other goals are:
* Avoid multi-threading. It uses a single thread approach to handle all the asynchronous operations and queue the communications' results in a independent queue for processing.
* Reduce memory fragmentation: It implements its own buffer allocation mecanism to help the garbage collector to 
deal with pinned memory.
* Low-memory usage. Internally, pooled objects and pre-allocated buffers are used to reduce the memory requirements.
* Low-grained bandwidth control. It allows to control the individual peer's upload and download rates.

Development
-----------
Peer2Net is developed by [Lucas Ontivero](http://geeks.ms/blogs/lontivero) ([@lontivero](http://twitter.com/lontivero)). You are welcome to contribute code. You can send code both as a patch or a GitHub pull request.

Note that Peer2Net is still very much work in progress. 
