Usage:  
  HtmlToPdf [GLOBAL OPTION]... [OBJECT]... <output file>  
  
Document objects:  
  HtmlToPdf is able to put several objects into the output file, an object is  
  either a single webpage, a cover webpage or a table of contents.  The objects  
  are put into the output document in the order they are specified on the  
  command line, options can be specified on a per object basis or in the global  
  options area. Options from the Global Options section can only be placed in  
  the global options area.  
  
  A page objects puts the content of a single webpage into the output document.  
  
  (page)? <input url/file name> [PAGE OPTION]...  
  Options for the page object can be placed in the global options and the page  
  options areas. The applicable options can be found in the Page Options and  
  Headers And Footer Options sections.  
  
  A cover objects puts the content of a single webpage into the output document,  
  the page does not appear in the table of contents, and does not have headers  
  and footers.  
  
  cover <input url/file name> [PAGE OPTION]...  
  All options that can be specified for a page object can also be specified for  
  a cover.  
  
  A table of contents object inserts a table of contents into the output  
  document.  
  
  toc [TOC OPTION]...  
  All options that can be specified for a page object can also be specified for  
  a toc, further more the options from the TOC Options section can also be  
  applied. The table of contents is generated via XSLT which means that it can  
  be styled to look however you want it to look. To get an idea of how to do  
  this you can dump the default xslt document by supplying the  
  --dump-default-toc-xsl, and the outline it works on by supplying  
  --dump-outline, see the Outline Options section.  
  
Description:  
  Converts one or more HTML pages into a PDF document, using headless Chrome.  
  
  --javascript-delay                               (Default: 200) The number of  
                                                   milliseconds to wait for  
                                                   javascript to finish  
  
  --log-level                                      (Default: Info) Set log level  
                                                   to: none, error, warn, info,  
                                                   or debug  
  
  --user-style-sheet                               Specify a user style sheet,  
                                                   to load with every page  
  
  --footer-left=<text>                             Left aligned footer text  
  
  --footer-center=<text>                           Centered footer text  
  
  --footer-right=<text>                            Right aligned footer text  
  
  --footer-font-size=<size>                        (Default: 12) Footer font  
                                                   size  
  
  --footer-font-name=<name>                        (Default: Arial) Footer font  
                                                   name  
  
  --footer-html=<url>                              HTML footer  
  
  --header-left=<text>                             Left aligned header text  
  
  --header-center=<text>                           Centered header text  
  
  --header-right=<text>                            Right aligned header text  
  
  --header-font-size=<size>                        (Default: 12) Header font  
                                                   size  
  
  --header-font-name=<name>                        (Default: Arial) Header font  
                                                   name  
  
  --header-html=<url>                              HTML header  
  
  --read-args-from-stdin                           Read command line arguments  
                                                   from stdin  
  
  -B <unitreal>, --margin-bottom=<unitreal>        Set the page bottom margin  
  
  -L <unitreal>, --margin-left=<unitreal>          (Default: 10mm) Set the page  
                                                   left margin  
  
  -R <unitreal>, --margin-right=<unitreal>         (Default: 10mm) Set the page  
                                                   right margin  
  
  -T <unitreal>, --margin-top=<unitreal>           Set the page top margin  
  
  -O <orientation>, --orientation=<orientation>    (Default: Portrait) Set  
                                                   orientation to Landscape or  
                                                   Portrait  
  
  -s <Size>, --page-size=<Size>                    (Default: A4) Set paper size  
                                                   to: A4, Letter, etc.  
  
  --page-height=<unitreal>                         Page height  
  
  --page-width=<unitreal>                          Page width  
  
  --background                                     (Default: true) Do print  
                                                   background  
  
  --no-background                                  Do not print background  
  
  --page-offset=<offset>                           (Default: 0) The starting  
                                                   page number  
  
  --title=<title>                                  The title of the generated  
                                                   pdf file. The title of the  
                                                   first document is used if not  
                                                   specified.  
  
  -h, --help                                       Display this help screen.  
  
  -V, --version                                    Display version information.  
  
  --dump-default-toc-xsl                           Dumps the default TOC XSL  
                                                   style sheet to the standard  
                                                   output (STDOUT) stream.  
  
  --dump-outline                                   Dump the outline to a file.  
  
  --enable-local-file-access                       Allowed conversion of a local  
                                                   file to read in other local  
                                                   files.  
  
  --disable-local-file-access                      Do not allowed conversion of  
                                                   a local file to read in other  
                                                   local files, unless  
                                                   explicitly allowed with  
                                                   --allow (default)  
  
  --disable-dotted-lines                           Do not use dotted lines in  
                                                   the toc  
  
  --additional-arguments                           Additional arguments to pass  
                                                   to the browser instance  
  
  
Page sizes:  
  The default page size of the rendered document is A4, but by using the  
  --page-size option this can be changed to almost anything else, such as: A3,  
  Letter and Legal.  For a full list of supported pages sizes please see  
  <https://qt-project.org/doc/qt-4.8/qprinter.html#PaperSize-enum>.  
  
  For a more fine grained control over the page size the --page-height and  
  --page-width options may be used  
  
Reading arguments from stdin:  
  If you need to convert a lot of pages in a batch, and you feel that  
  HtmlToPdf is a bit too slow to start up, then you should try  
  --read-args-from-stdin,  
  
  When --read-args-from-stdin each line of input sent to HtmlToPdf on stdin  
  will act as a separate invocation of HtmlToPdf, with the arguments specified  
  on the given line combined with the arguments given to HtmlToPdf  
  
  For example one could do the following:  
  
  echo "https://qt-project.org/doc/qt-4.8/qapplication.html qapplication.pdf" >>  
  cmds  
  echo "cover google.com https://en.wikipedia.org/wiki/Qt_(software) qt.pdf" >>  
  cmds  
  HtmlToPdf --read-args-from-stdin --book < cmds  
  
Footers And Headers:  
  Headers and footers can be added to the document by the --header-* and  
  --footer* arguments respectively.  In header and footer text string supplied  
  to e.g. --header-left, the following variables will be substituted.  
  
   * [page]       Replaced by the number of the pages currently being printed  
   * [frompage]   Replaced by the number of the first page to be printed  
   * [topage]     Replaced by the number of the last page to be printed  
   * [webpage]    Replaced by the URL of the page being printed  
   * [section]    Replaced by the name of the current section  
   * [subsection] Replaced by the name of the current subsection  
   * [date]       Replaced by the current date in system local format  
   * [isodate]    Replaced by the current date in ISO 8601 extended format  
   * [time]       Replaced by the current time in system local format  
   * [title]      Replaced by the title of the of the current page object  
   * [doctitle]   Replaced by the title of the output document  
   * [sitepage]   Replaced by the number of the page in the current site being  
   converted  
   * [sitepages]  Replaced by the number of pages in the current site being  
   converted  
  
  
  As an example specifying --header-right "Page [page] of [topage]", will result  
  in the text "Page x of y" where x is the number of the current page and y is  
  the number of the last page, to appear in the upper left corner in the  
  document.  
  
  Headers and footers can also be supplied with HTML documents. As an example  
  one could specify --header-html header.html, and use the following content in  
  header.html:  
  
  <!DOCTYPE html>  
  <html><head><script>  
  function subst() {  
      var vars = {};  
      var query_strings_from_url =  
      document.location.search.substring(1).split('&');  
      for (var query_string in query_strings_from_url) {  
          if (query_strings_from_url.hasOwnProperty(query_string)) {  
              var temp_var = query_strings_from_url[query_string].split('=', 2);  
              vars[temp_var[0]] = decodeURI(temp_var[1]);  
          }  
      }  
      var css_selector_classes = ['page', 'frompage', 'topage', 'webpage',  
      'section', 'subsection', 'date', 'isodate', 'time', 'title', 'doctitle',  
      'sitepage', 'sitepages'];  
      for (var css_class in css_selector_classes) {  
          if (css_selector_classes.hasOwnProperty(css_class)) {  
              var element =  
              document.getElementsByClassName(css_selector_classes[css_class]);  
              for (var j = 0; j < element.length; ++j) {  
                  element[j].textContent =  
                  vars[css_selector_classes[css_class]];  
              }  
          }  
      }  
  }  
  </script></head><body style="border:0; margin: 0;" onload="subst()">  
  <table style="border-bottom: 1px solid black; width: 100%">  
    <tr>  
      <td class="section"></td>  
      <td style="text-align:right">  
        Page <span class="page"></span> of <span class="topage"></span>  
      </td>  
    </tr>  
  </table>  
  </body></html>  
  
  
  As can be seen from the example, the arguments are sent to the header/footer  
  html documents in get fashion.  
  
Outlines:  
  HtmlToPdf has support for PDF outlines also known as book  
  marks, this can be enabled by specifying the --outline switch. The outlines  
  are generated based on the <h?> tags, for a in-depth description of how this  
  is done see the Table Of Contents section.  
  
  The outline tree can sometimes be very deep, if the <h?> tags where spread to  
  generous in the HTML document.  The --outline-depth switch can be used to  
  bound this.  
  
Table Of Contents:  
  A table of contents can be added to the document by adding a toc object to the  
  command line. For example:  
  
  HtmlToPdf toc https://qt-project.org/doc/qt-4.8/qstring.html qstring.pdf  
  
  The table of contents is generated based on the H tags in the input documents.  
  First a XML document is generated, then it is converted to HTML using XSLT.  
  
  The generated XML document can be viewed by dumping it to a file using the  
  --dump-outline switch. For example:  
  
  HtmlToPdf --dump-outline toc.xml  
  https://qt-project.org/doc/qt-4.8/qstring.html qstring.pdf  
  
  The XSLT document can be specified using the --xsl-style-sheet switch. For  
  example:  
  
  HtmlToPdf toc --xsl-style-sheet my.xsl  
  https://qt-project.org/doc/qt-4.8/qstring.html qstring.pdf  
  
  The --dump-default-toc-xsl switch can be used to dump the default XSLT style  
  sheet to stdout. This is a good start for writing your own style sheet  
  
  HtmlToPdf --dump-default-toc-xsl  
  The XML document is in the namespace "http://wkhtmltopdf.org/outline" it has a  
  root node called "outline" which contains a number of "item" nodes. An item  
  can contain any number of item. These are the outline subsections to the  
  section the item represents. A item node has the following attributes:  
  
 * "title" the name of the section.  
 * "page" the page number the section occurs on.  
 * "link" a URL that links to the section.  
 * "backLink" the name of the anchor the section will link back to.  
  
  The remaining TOC options only affect the default style sheet so they will not  
  work when specifying a custom style sheet.  
