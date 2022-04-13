using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Models.Group;

namespace Server.Database.Models.Mail;

public class MailAccountGroupAccessModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string MailAccountModelMailAddress { get; set; }

    public MailAccountModel  MailAccountModel { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int GroupModelId { get; set; }
    public GroupModel GroupModel { get; set; }

    public bool Owner { get; set; }
}