using System.Collections.Generic;
using System.Linq;

public class TimedLog
{
    private class Entry
    {
        public string Text;
        public double Timestamp; // seconds since epoch
        public Entry(string text, double timestamp)
        {
            Text = text;
            Timestamp = timestamp;
        }
    }

    private readonly List<Entry> _entries = new List<Entry>();
    private readonly double _lifespan; // seconds

    public TimedLog(double lifespanSeconds)
    {
        _lifespan = lifespanSeconds;
    }

    public void Add(string message)
    {
        double now = (System.DateTime.UtcNow - new System.DateTime(1970,1,1)).TotalSeconds;
        _entries.Add(new Entry(message, now));
    }

    public void Update()
    {
        double now = (System.DateTime.UtcNow - new System.DateTime(1970,1,1)).TotalSeconds;
        _entries.RemoveAll(e => now - e.Timestamp > _lifespan);
    }

    public List<string> GetEntries()
    {
        return _entries.Select(e => e.Text).ToList();
    }

    public override string ToString()
    {
        string value = "";
        foreach (var entry in _entries)
        {
            value += entry.Text + "\n";
        }

        return value;
    }
}