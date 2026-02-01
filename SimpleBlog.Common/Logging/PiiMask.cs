namespace SimpleBlog.Common.Logging;

public static class PiiMask
{
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "unknown";
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            return "unknown";
        }

        var firstChar = email[..1];
        var domain = email[(atIndex + 1)..];
        return $"{firstChar}***@{domain}";
    }

    public static string MaskUserName(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "unknown";
        }

        return username.Length <= 2
            ? $"{username[0]}*"
            : $"{username[..1]}***";
    }

    public static string MaskSubject(string? subject) => MaskUserName(subject);
}
