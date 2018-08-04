module brokendiscord.client;

abstract class Client 
{
    /* Client authentication token. */
    string token;

    ShardInfo* shardInfo;

    Gateway gateway;

    this(string token, ShardInfo* shardInfo = null) 
    {
        this.token = token;
        this.shardInfo = shardInfo ? shardInfo : new ShardInfo();

        this.gateway = new Gateway(this);
    }

    public void run()
    {

    }
}