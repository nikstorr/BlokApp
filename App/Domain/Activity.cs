namespace App.Domain;

public class Activity
{
    /* 
     This a Data transfer object (DTO) for activities.
     It is not necessary to use a class for this as there are no app boundaries to cross.
    
     TODO Consider using a DataTable directly
     */


    // KLA from the group
    public string KLA { get; set; }
    // BLOK from the group
    public string AKT_NAVN { get; set; }
    // number of identical POS columns in the activity
    public int POS { get; set; }
    // PER ciphers from the group
    public string PER { get; set; }
}
