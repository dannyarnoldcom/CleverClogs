public struct UXMLAsset 
{
    public string path;
    public string name;
    public bool selected;

    public UXMLAsset(string path, string name, bool selected)
    {
        this.path = path;
        this.name = name;
        this.selected = selected;
    }
}
