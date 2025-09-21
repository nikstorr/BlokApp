using System.Data;

namespace App.Domain;

public class Block
{
    // Key is (KLA, BLOK)
    public (string, string) Key { get; set; }
    // Rows in the block
    public List<DataRow> Rows{ get; set; }

    public Block()
    {
        Rows = [];
    }
}

