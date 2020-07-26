using System;
using System.Linq;

namespace WeeklyReddit.Services
{
    public static class DataFormatter
    {
        private static string GetTitle(RedditPost post) =>
            post.Nsfw ? $"{post.Title} [NSFW]" : post.Title;

        private static string GetImageUrl(RedditPost post) =>
            string.IsNullOrEmpty(post.ImageUrl) ? "https://www.redditinc.com/assets/images/site/logo.svg" : post.ImageUrl;

        private static string BigPostTemplate(string subreddit, RedditPost post) => $@"
    <table border=""0"" width=""100%"" cellpadding=""0"" cellspacing=""0"" bgcolor=""ffffff"">
      <tbody>
        <tr>
          <td height=""15"" style=""font-size: 15px; line-height: 15px;"">&nbsp;</td>
        </tr>
        <tr>
          <td align=""center"">
            <table border=""0"" align=""center"" width=""590"" cellpadding=""0"" cellspacing=""0"" class=""container590"">
              <tbody>
                <tr>
                  <td>
                    <table
                      border=""0""
                      width=""590""
                      align=""left""
                      cellpadding=""0""
                      cellspacing=""0""
                      style=""border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt;""
                      class=""container590""
                    >
                      <tbody>
                        <tr>
                          <td align=""center"" class=""section-img"">
                            <a href=""{post.CommentsUrl}"" style="" border-style: none !important; border: 0 !important;""
                              ><img
                                src=""{GetImageUrl(post)}""
                                style=""display: block; width: 590px;""
                                width=""590""
                                border=""0""
                                alt=""""
                            /></a>
                          </td>
                        </tr>

                        <tr>
                          <td height=""20"" style=""font-size: 20px; line-height: 20px;"">&nbsp;</td>
                        </tr>

                        <tr>
                          <td
                            align=""left""
                            style=""color: #212a2e; font-size: 18px; font-family: 'Source Sans Pro', Helvetica, Calibri, sans-serif; font-weight: 500; line-height: 24px;""
                            class=""align-center outlook-font""
                          >
                            {GetTitle(post)}
                          </td>
                        </tr>

                        <tr>
                          <td height=""15"" style=""font-size: 15px; line-height: 15px;"">&nbsp;</td>
                        </tr>

                        <tr>
                          <td
                            align=""left""
                            style=""color: #838383; font-family: 'Source Sans Pro', Helvetica, Calibri, sans-serif; line-height: 20px;font-size:14px;""
                            class=""align-center outlook-font""
                          >
                            <div style=""line-height: 20px"">
                              Score: {post.Score} Comments: {post.Comments} Subreddit: {subreddit}
                            </div>
                          </td>
                        </tr>

                        <tr>
                          <td height=""15"" style=""font-size: 15px; line-height: 15px;"">&nbsp;</td>
                        </tr>

                        <tr>
                          <td align=""left"">
                            <table border=""0"" align=""left"" cellpadding=""0"" cellspacing=""0"" class=""container590"">
                              <tbody>
                                <tr>
                                  <td align=""center"">
                                    <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                                      <tbody>
                                        <tr>
                                          <td
                                            align=""center""
                                            style=""color: #5c9085; font-family: 'Source Sans Pro', Helvetica, Calibri, sans-serif; line-height: 22px;""
                                            class=""outlook-font""
                                          >
                                            <div style=""line-height: 22px;"">
                                              <a href=""{post.CommentsUrl}"" style=""color: #5c9085; text-decoration: none;"">Read more</a>
                                            </div>
                                          </td>
                                        </tr>
                                      </tbody>
                                    </table>
                                  </td>
                                </tr>
                              </tbody>
                            </table>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>
          </td>
        </tr>
        <tr>
          <td height=""15"" style=""font-size: 15px; line-height: 15px;"">&nbsp;</td>
        </tr>
      </tbody>
    </table>";

        private static string TrendingTemplate(RedditPost post) => @$"
    <table border=""0"" width=""100%"" cellpadding=""0"" cellspacing=""0"" bgcolor=""ffffff"">
      <tbody>
        <tr>
          <td height=""25"" style=""font-size: 25px; line-height: 25px;"">&nbsp;</td>
        </tr>

        <tr>
          <td align=""center"">
            <table border=""0"" align=""center"" width=""590"" cellpadding=""0"" cellspacing=""0"" class=""container590"">
              <tbody>
                <tr>
                  <td>
                    <table
                      border=""0""
                      width=""590""
                      align=""left""
                      cellpadding=""0""
                      cellspacing=""0""
                      style=""border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt;""
                      class=""container590""
                    >
                      <tbody>
                        <tr>
                          <td
                            align=""left""
                            style=""color: #212a2e; font-size: 18px; font-family: 'Source Sans Pro', Helvetica, Calibri, sans-serif; font-weight: 500; line-height: 24px;""
                            class=""align-center outlook-font""
                          >
                            {post.Title}
                          </td>
                        </tr>
                        <tr>
                          <td align=""left"">
                            <table border=""0"" align=""left"" cellpadding=""0"" cellspacing=""0"" class=""container590"">
                              <tbody>
                                <tr>
                                  <td align=""center"">
                                    <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                                      <tbody>
                                        <tr>
                                          <td
                                            align=""center""
                                            style=""color: #5c9085; font-family: 'Source Sans Pro', Helvetica, Calibri, sans-serif; line-height: 22px;""
                                            class=""outlook-font""
                                          >
                                            <div style=""line-height: 22px;"">
                                              <a href=""{post.Url}"" style=""color: #5c9085; text-decoration: none;"" target=""_blank"" rel=""noopener"">Read more</a>
                                            </div>
                                          </td>
                                        </tr>
                                      </tbody>
                                    </table>
                                  </td>
                                </tr>
                              </tbody>
                            </table>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>
          </td>
        </tr>
      </tbody>
    </table>";

        private static string HeaderTemplate(string title, DateTime issueDate) => @$"
    <table border=""0"" width=""100%"" cellpadding=""0"" cellspacing=""0"" bgcolor=""5c9085"">
      <tr>
        <td
          align=""center""
          style=""background-image: url(https://i.imgur.com/9fZc2Wu.png); background-size: cover; background-position: top center; background-repeat: no-repeat;""
          background=""https://i.imgur.com/9fZc2Wu.png""
        >
          <table border=""0"" align=""center"" width=""590"" cellpadding=""0"" cellspacing=""0"" class=""container590"">
            <tr>
              <td height=""50"" style=""font-size: 50px; line-height: 50px;"">&nbsp;</td>
            </tr>

            <tr>
              <td align=""center"">
                <table
                  border=""0""
                  width=""500""
                  align=""center""
                  cellpadding=""0""
                  cellspacing=""0""
                  style=""border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt;""
                  class=""container590""
                >
                  <tr>
                    <td
                      align=""center""
                      style=""color: #ffffff; font-size: 45px; font-family: 'Titillium Web', Helvetica Neue, Calibri, sans-serif; line-height: 35px;text-shadow: black 0.1em 0.1em 0.2em;""
                      class=""main-section-header outlook-font""
                    >
                      <div style=""line-height: 35px"">
                        {title}
                      </div>
                    </td>
                  </tr>

                  <tr>
                    <td height=""20"" style=""font-size: 20px; line-height: 20px;"">&nbsp;</td>
                  </tr>

                  <tr>
                    <td
                      align=""center""
                      class=""outlook-font""
                      style=""color: #eaf5ff; font-size: 15px; font-family: 'Titillium Web', Helvetica Neue, Calibri, sans-serif; line-height: 24px;text-shadow: black 0.1em 0.1em 0.2em;""
                    >
                      <div style=""line-height: 24px"">
                        Issue {issueDate.ToLongDateString()}
                      </div>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>

            <tr>
              <td height=""60"" style=""font-size: 60px; line-height: 60px;"">&nbsp;</td>
            </tr>
          </table>
        </td>
      </tr>
    </table>";

        private static string BodyTemplate(FormatterOptions options) => @$"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns:v=""urn:schemas-microsoft-com:vml"">
  <head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width; initial-scale=1.0; maximum-scale=1.0;"" />
    <meta name=""viewport"" content=""width=600,initial-scale = 2.3,user-scalable=no"" />
    <!--[if !mso]><!-- -->
    <link href=""https://fonts.googleapis.com/css?family=Titillium+Web"" rel=""stylesheet"" />
    <link href=""https://fonts.googleapis.com/css?family=Source+Sans+Pro"" rel=""stylesheet"" />
    <!--<![endif]-->

    <title>{options.Title}</title>

    <style type=""text/css"">
      html,
      body {{
        -webkit-font-smoothing: antialiased;
        width: 100%;
        padding: 0;
        margin: 0;
      }}

      @media only screen and (max-width: 640px) {{
        /*------ top header ------ */
        .header-main {{
          font-size: 22px !important;
        }}
        .main-section-header {{
          font-size: 28px !important;
        }}
        .show {{
          display: block !important;
        }}
        .hide {{
          display: none !important;
        }}
        .align-center {{
          text-align: center !important;
        }}
        .main-image img {{
          width: 440px !important;
          height: auto !important;
        }}
        .container590 {{
          width: 440px !important;
        }}
        .half-container {{
          width: 220px !important;
        }}
        .main-button {{
          width: 220px !important;
        }}
        .section-img img {{
          width: 320px !important;
          height: auto !important;
        }}
      }}

      @media only screen and (max-width: 479px) {{
        .header-main {{
          font-size: 20px !important;
        }}
        .main-section-header {{
          font-size: 26px !important;
        }}
        .container590 {{
          width: 280px !important;
        }}
        .container590 {{
          width: 280px !important;
        }}
        .half-container {{
          width: 130px !important;
        }}
        .section-img img {{
          width: 280px !important;
          height: auto !important;
        }}
      }}
    </style>
    <!--[if gte mso 9]><style type=”text/css”>
        .outlook-font {{
          font-family: arial, sans-serif!important;
        }}
        </style>
    <![endif]-->
  </head>

  <body leftmargin=""0"" topmargin=""0"" marginwidth=""0"" marginheight=""0"">
    <!-- ======= Pre-header ======= -->
    <table style=""display:none!important;"">
      <tr>
        <td>
          <div
            style=""overflow:hidden;display:none;font-size:1px;color:#ffffff;line-height:1px;font-family:Arial;maxheight:0px;max-width:0px;opacity:0;""
          >
            Pre-header for the newsletter template
          </div>
        </td>
      </tr>
    </table>
    <!-- ======= Pre-header end ======= -->
    {HeaderTemplate(options.Title, options.IssueDate)}
    {string.Join(string.Empty, options.Trendings.Select(TrendingTemplate))}
    {string.Join(string.Empty, options.Subreddits.SelectMany(x => x.TopPosts.Select(z => BigPostTemplate(x.Name, z))))}
  </body>
</html>
";
        public static string GenerateHtml(FormatterOptions options) => BodyTemplate(options);
    }
}