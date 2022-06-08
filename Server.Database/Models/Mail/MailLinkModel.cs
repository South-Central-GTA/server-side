namespace Server.Database.Models.Mail;

public class MailLinkModel
{
    public string MailAccountModelMailAddress { get; set; }
    public MailAccountModel MailAccountModel { get; set; }

    public int MailModelId { get; set; }
    public MailModel MailModel { get; set; }
    public bool IsAuthor { get; set; }
}