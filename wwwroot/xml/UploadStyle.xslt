<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes" />

  <xsl:template match="/">
    <table class="table table-success table-striped">
      <thead>
        <tr>
          <th>Id</th>
          <th>Name</th>
          <th>Value</th>
        </tr>
      </thead>
      <tbody>
        <xsl:for-each select="Root/Record">
          <tr>
            <td>
              <xsl:value-of select="Id"/>
            </td>
            <td>
              <b><xsl:value-of select="Name"/></b>
            </td>
            <td>
              <xsl:value-of select="Value"/>
            </td>
          </tr>
        </xsl:for-each>
      </tbody>
    </table>
  </xsl:template>
</xsl:stylesheet>