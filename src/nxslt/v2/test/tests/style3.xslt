<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
xmlns:ext="foo">    
  <xsl:output encoding="iso-8859-1"/>
  <xsl:template match="/">
      <out>
	<xsl:value-of select="ext:Add(2,3)"/>
      </out>    
  </xsl:template>

</xsl:stylesheet>