module brokendiscord.gateway.gateway;

/* Gateway version */
const ubyte GATEWAY_VERSION = 6;

class Gateway
{
    Client client;

    WebSocket webSock;

    string sessionID;

    uint sequenceNumber;

    uint heartbeatInterval;

    bool connected;

    uint reconnects;

    Task heartbeater;

    Emitter eventEmitter;

    private string cachedGatewayURL;
    private Couunter!string eventCounter;
    private bool eventTracking;

    this(Client client, bool eventTracking = false)
    {
        this.client = client;
        this.eventTracking = eventTracking;

        this.eventEmitter = new Emitter;
        this.eventEmitter.listen!Ready(toDelegate(&this.handleReadyEvent));
        this.eventEmitter.listen!Resumed(toDelegate(&this.handleResumedEvent);

        //client.events = this.eventEmitter;

        if (this.eventTracking) {
            this.eventCounter = new Counter!string;
        }
    }

    void start()
    {
        if(this.webSock && this.webSock.connected) this.webSock.close();

        // TODO: Set gateway url.. 
        this.sock = connectWebSocket(URL(this.cachedGatewayURL));
        runTask(toDelegate(&this.run));
    }
}