<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <table class="table table-bordered table-striped">
      <thead class="table-dark">
        <tr>
          <th>ID</th>
          <th>Title</th>
          <th>Author</th>
          <th>Date</th>
        </tr>
      </thead>
      <tbody>
        <xsl:for-each select="Documents/Document">
          <tr>
            <td><xsl:value-of select="Id"/></td>
            <td><xsl:value-of select="Title"/></td>
            <td><xsl:value-of select="Author"/></td>
            <td><xsl:value-of select="Date"/></td>
          </tr>
        </xsl:for-each>
      </tbody>
    </table>
  </xsl:template>
</xsl:stylesheet>