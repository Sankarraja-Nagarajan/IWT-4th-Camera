using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWT.Shared
{
    internal enum HtmlTokenType
    {
        OpeningTagStart,
        ClosingTagStart,
        TagEnd,
        EmptyTagEnd,
        EqualSign,
        Name,
        Atom, // any attribute value not in quotes
        Text, //text content when accepting text
        Comment,
        EOF,
    }
}
