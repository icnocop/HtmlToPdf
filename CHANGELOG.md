# Change Log

__v0.1.704__ (2022-07-04)

 - Added retry logic if an IOException occurs as a result of trying to delete a file.  
 - Specifying temporary directory for `UserDataDir` to improve performance

__v0.1.328__ (2022-03-28)

 - Added retry logic if a PuppeteerSharp exception occurs as a result of an iTextSharp InvalidPdfException: PDF header signature not found.  

__v0.1.302.37__ (2022-03-02)

 - Updated third party dependencies  

__v0.0.1013.1__ (2020-10-13)

 - Added WebSocket factory implementation to support Windows 7 and Windows Server 2008  

__v0.0.1007.7__ (2020-10-07)

 - Work-around for issue where puppeteer/chromium may timeout waiting for navigation after 30 seconds  

__v0.0.921.0__ (2020-09-21)

 - Work-around for issue where puppeteer/chromium inserts margins in the header of the first page, affecting PDF page counts  

__v0.0.920.3__ (2020-09-20)

 - Retry launching chrome if an exception occurs  
 - Added more application specific exceptions  
 - Added more debugging  

__v0.0.812.0__ (2020-08-12)

 - Fixed issue when using multiple input files

__v0.0.810.12__ (2020-08-10)

 - Added support for `--disable-dotted-lines` command line option

__v0.0.810.9__ (2020-08-10)

 - Added support for `toc` command line option

__v0.0.802.1__ (2020-08-01)

 - Added support for `--dump-outline` command line option

__v0.0.714.0__ (2020-07-14)

 - Added support for `--enable-local-file-access` and `--disable-local-file-access` command line options

__v0.0.612.2__ (2020-06-12)

 - Fixed issue when referencing relative file paths in HTML files

__v0.0.609.0__ (2020-06-09)

 - Added support for `--dump-default-toc-xsl` command line options

__v0.0.526.4__ (2020-05-26)

 - Added support for `-h` and `v` command line options

__v0.0.526.2__ (2020-05-26)

 - Separated HTML to PDF processing logic to HtmlToPdf.dll and created HtmlToPdf.Console.exe

__v0.0.526.1__ (2020-05-26)

 - Added support for the `--title` command line option
 - Added support for `frompage`, `topage`, `isodate`, `time`, and `doctitle` header/footer variables

__v0.0.524.5__ (2020-05-24)

 - Added support for the `--page-offset` command line option
 
__v0.0.524.4__ (2020-05-24)

 - Added support for `--footer-font-size`, `--footer-font-name`, and `--footer-html` command line options

__v0.0.524.2__ (2020-05-24)

 - Added support for `--footer-left` and `--footer-right` command line options

__v0.0.518.3__ (2020-05-18)

 - Fixed issues with footer template builder
 - Added support for the `[date]`, `[title]`, and `[webpage]` variables in the header/footer

__v0.0.517.2__ (2020-05-17)

 - Improved performance

__v0.0.516.0__ (2020-05-16)

 - Added support for custom footer styles

__v0.0.515.0__ (2020-05-15)

 - Added support for the `--log-level` command line option

__v0.0.503.0__ (2020-05-03)

 - Excluded footer on cover page
 - Log warning if the user style sheet file doesn't exist instead of throwing an exception

__v0.0.427.0__ (2020-04-27)

 - Fixed page numbers with multiple HTML file inputs

__v0.0.420.5__ (2020-04-20)

 - Fixed issues with directory separator and case sensitivity

__v0.0.419.0__ (2020-04-19)

 - Fixed issue with setting the page width and height

__v0.0.416.0__ (2020-04-16)

 - Fixed issues with the background command line options

__v0.0.410.2__ (2020-04-10)

 - Replaced external file links to internal page links after merging

__v0.0.409.11__ (2020-04-09)

 - Initial release