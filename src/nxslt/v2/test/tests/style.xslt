<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
xmlns:date="http://exslt.org/dates-and-times" xmlns:exsl="http://exslt.org/common">    
  <xsl:output encoding="iso-8859-1"/>
  <xsl:template match="/">
      <out>
	<xsl:value-of select="date:date-time()"/>
	<xsl:copy-of select="/"/>
	<exsl:document href="second.xml" encoding="ascii">
           <second>document</second>
        </exsl:document>        
      </out>    
  </xsl:template>

</xsl:stylesheet>