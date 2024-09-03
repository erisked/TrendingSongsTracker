interface IDataSource
{
    public void SyncLiveEntries(string evicted);
    public void Flush(int maxQueuelength);
}