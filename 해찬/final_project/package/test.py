import pymssql

con = pymssql.connect(#server="192.168.0.112:1433",
                    #   host='localhost',
                      server = "222.235.141.8:1111",
                      database="KDTB03_1JO",
                      user="KDTB03",
                      password="333538",
                      charset = "EUC-KR")

rows = []
cur=con.cursor()
sql = ""
sql += "SELECT CODENAME"
sql += "      ,MINORCODE"
sql += "  FROM TB_Standard "
sql += " WHERE MAJORCODE = 'WHCODE' "
cur.execute(sql)
rows = cur.fetchall()
con.close()
new_list = []
print(rows)