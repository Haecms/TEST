from flask import Flask, render_template, request, jsonify
import package.pkg_scale2 as pb2
import base64


app = Flask(__name__)

# 메인 홈 화면
@app.route("/", methods=['GET', 'POST'])
def start():
    rows = pb2.get_warehouse()  # 창고 콤보박스에 컬럼들을 채움
    rows2 = pb2.get_name()      # 작업자 콤보박스에 컬럼들을 채움
    plantcode = "창고"
    if request.method == 'POST': # html에서 조회 버튼을 클릭했을 때 
        selected_option = request.form.get('selected_option') # 선택된 창고code를 가져옴
        current = pb2.get_current(selected_option)  # 선택된 창고에 품목정보를 가져옴
        current_change = []
        for i in range(len(current)):
            current_in =[]
            for j in range(len(current[i])):
                if j != len(current[i]):
                    try:
                        current_in.append(format(int(current[i][j] ),',' ))
                    except:
                        current_in.append(current[i][j])
                else:
                    try:
                        pass
                        # 여기서 current[i][4]에 담겨있는 바이너리 패스를 처리해야된다.
                        src = current[i][j]
                        binsrc_encode = src.encode()
                        img_b64 = base64.b64encode(binsrc_encode)
                        img_decode_ascii = img_b64.decode('ascii')
                        current_in.append(img_decode_ascii)
                    except:
                        current_in.append('에러발생ㅠㅠ')
                    
            current_change.append(current_in)

        plantcode = ["창고"]
        if selected_option != "none":
            plantcode = pb2.get_warehouse(selected_option)

    # current_change(list(이중리스트)) : 
        return render_template("auto_scale2.html", items = rows, itemcode1 = current_change, items2 = rows2, plantcode = plantcode[0]) # 원자재창고 검색할 경우 6개가 나옴
    return render_template("auto_scale2.html", items = rows, items2 = rows2, plantcode=plantcode) 



# 발주 버튼 클릭
@app.route("/fetch_data_insert")
def auto_button_insert():
    itemcode = request.args.get('itemcode')
    autoqty = request.args.get('autoqty')
    WorkerID = request.args.get('WorkerID')
    s = pb2.auto_enroll(itemcode, autoqty, WorkerID)
    return jsonify({'message':f'{s}'})



if __name__ == "__main__":
    app.run(host="localhost", port=5000, debug=True)