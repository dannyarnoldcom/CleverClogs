public struct SearchFolder 
{
    public string path;
    public bool selected;

    public SearchFolder(string path, bool selected)
    {
        this.path = path;
        this.selected = selected;
    }
}
