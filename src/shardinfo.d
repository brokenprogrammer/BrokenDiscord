module brokendiscord.shardinfo;
 
 /**
  * Struct containing information regarding gateway sharding.
  */
struct ShardInfo {
    // This shard's number.
    ushort shard = 0;

    // Total number of shards.
    ushort totalShards = 1;
}