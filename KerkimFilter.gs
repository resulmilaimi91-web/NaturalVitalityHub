function onOpen() {
  SpreadsheetApp.getUi().createMenu('KërkimFiltër')
    .addItem('Kërko në të gjitha sheet-et', 'showSearchDialog')
    .addItem('Filtro sheet-in aktiv', 'filterActiveSheet')
    .addSeparator()
    .addItem('Fshi filtrat', 'clearFilters')
    .addItem('Raporto (HTML)', 'showReportDialog')
    .addToUi();
}

function showSearchDialog() {
  SpreadsheetApp.getUi().showModalDialog(
    HtmlService.createHtmlOutput(SEARCH_HTML).setWidth(500).setHeight(400),
    'Kërko në të Gjitha Sheet-et');
}

function showReportDialog() {
  SpreadsheetApp.getUi().showModalDialog(
    HtmlService.createHtmlOutput(REPORT_HTML).setWidth(950).setHeight(600),
    'Raporto');
}

function filterActiveSheet() {
  var s = SpreadsheetApp.getActiveSheet(), r = s.getDataRange(), v = r.getValues();
  if (v.length < 2) return;
  var h = v[0], d = v.slice(1);
  var html = '<style>body{font-family:Segoe UI;background:#1e1e23;color:#c8cdd6;padding:16px}input{width:100%;padding:8px;margin:6px 0;border:1px solid #444;border-radius:4px;background:#2a2c33;color:#fff}button{padding:8px 20px;background:#0064b0;color:#fff;border:none;border-radius:4px;cursor:pointer}table{width:100%;border-collapse:collapse;margin-top:12px}th{background:#003d66;color:#fff;padding:6px}td{padding:6px;border-bottom:1px solid #333}</style>'+
    '<h2>Filtro: '+s.getName()+'</h2>'+
    h.map(function(v,i){return '<label>'+v+'<input id="f_'+i+'" placeholder="Kërko..."></label>'}).join('')+
    '<button onclick="f()">Filtro</button> <button onclick="c()">Pastro</button>'+
    '<div id="c"></div><div id="r"></div>'+
    '<script>var h='+JSON.stringify(h)+',d='+JSON.stringify(d)+';'+
    'function f(){var fl=h.map(function(v,i){return document.getElementById("f_"+i).value.toLowerCase()});'+
    'var r=d.filter(function(x){return fl.every(function(f,i){return !f||String(x[i]).toLowerCase().includes(f)})});'+
    'document.getElementById("c").textContent="Gjetur: "+r.length+" nga "+d.length;'+
    'var t="<table><tr><th>"+h.join("</th><th>")+"</th></tr>";'+
    'r.forEach(function(x){t+="<tr><td>"+x.join("</td><td>")+"</td></tr>"});'+
    'document.getElementById("r").innerHTML=t+"</table>"}'+
    'function c(){h.forEach(function(v,i){document.getElementById("f_"+i).value=""});f()};f()</script>';
  SpreadsheetApp.getUi().showModalDialog(HtmlService.createHtmlOutput(html).setWidth(950).setHeight(650), 'Filtro');
}

function clearFilters() {
  var s = SpreadsheetApp.getActiveSheet();
  if (s.getFilter()) { s.getFilter().remove(); SpreadsheetApp.getUi().alert('Filtrat u fshinë.'); }
}

function searchAllSheets(q) {
  if (!q || !q.trim()) return { error: 'Shkruani një term.' };
  q = q.toLowerCase().trim();
  var ss = SpreadsheetApp.getActiveSpreadsheet(), sheets = ss.getSheets(), results = [], total = 0;
  for (var s = 0; s < sheets.length; s++) {
    var sheet = sheets[s], name = sheet.getName(), values = sheet.getDataRange().getValues();
    if (values.length < 2) continue;
    var headers = values[0], data = values.slice(1);
    for (var r = 0; r < data.length; r++) {
      for (var c = 0; c < data[r].length; c++) {
        if (String(data[r][c]).toLowerCase().includes(q)) {
          var rowData = {};
          for (var c2 = 0; c2 < headers.length; c2++) rowData[headers[c2]] = data[r][c2];
          results.push({ sheetName: name, rowIndex: r + 2, data: rowData });
          total++; break;
        }
      }
    }
  }
  return { results: results, total: total, sheets: sheets.length, query: q };
}

function getSpreadsheetData() {
  var ss = SpreadsheetApp.getActiveSpreadsheet(), sheets = ss.getSheets(), allData = [];
  for (var s = 0; s < sheets.length; s++) {
    var sheet = sheets[s], values = sheet.getDataRange().getValues();
    if (values.length < 2) continue;
    allData.push({ name: sheet.getName(), headers: values[0], rows: values.slice(1), totalRows: values.length - 1 });
  }
  return allData;
}

function activateSheetAndRow(sheetName, rowIndex) {
  var ss = SpreadsheetApp.getActiveSpreadsheet(), sheet = ss.getSheetByName(sheetName);
  if (sheet) { ss.setActiveSheet(sheet); sheet.setActiveRange(sheet.getRange(rowIndex, 1, 1, sheet.getLastColumn())); }
}

var SEARCH_HTML = '<!DOCTYPE html><html><head><base target="_top"><style>'+
'body{font-family:Segoe UI;background:#1e1e23;color:#c8cdd6;padding:16px}'+
'input{flex:1;padding:10px;border:1px solid #3a3c44;border-radius:6px;background:#2a2c33;color:#fff;font-size:14px}'+
'button{padding:10px 20px;background:#0064b0;color:#fff;border:none;border-radius:6px;cursor:pointer}'+
'.filters button{padding:4px 12px;background:#2a2c33;color:#8ab;border:1px solid #3a3c44;border-radius:12px;cursor:pointer;margin:3px}'+
'.filters button.active{background:#0064b0;color:#fff}'+
'.result-item{background:#25272e;border:1px solid #333;border-radius:6px;padding:10px;margin:4px 0}'+
'.sheet-name{color:#4a8abc;font-size:11px;font-weight:600}'+
'.field{font-size:13px;display:inline-block;width:48%}'+
'.label{color:#6a7a8a;min-width:80px}'+
'.highlight{background:#3a5a3a;padding:0 2px;border-radius:2px}'+
'.stats{color:#6a7a8a;font-size:12px}</style></head><body>'+
'<div style="display:flex;gap:8px;margin-bottom:12px">'+
'<input id="s" placeholder="Kërko: nr personal, emër, mbiemër..." onkeyup="if(event.key==\'Enter\')k()">'+
'<button onclick="k()">Kërko</button></div>'+
'<div class="filters">'+
'<button class="active" onclick="l(this,\'\')">Të gjitha</button>'+
'<button onclick="l(this,\'emer\')">Emri</button>'+
'<button onclick="l(this,\'mbiemer\')">Mbiemri</button>'+
'<button onclick="l(this,\'nr\')">Nr Personal</button>'+
'<button onclick="l(this,\'telefon\')">Telefon</button>'+
'<button onclick="l(this,\'email\')">Email</button></div>'+
'<div id="st" class="stats">Shkruani një term</div><div id="res"></div>'+
'<script>var fl=\'\';'+
'function l(b,f){document.querySelectorAll(\'.filters button\').forEach(function(x){x.classList.remove(\'active\')});b.classList.add(\'active\');fl=f;k()}'+
'function h(t,q){if(!q)return String(t);return String(t).replace(new RegExp(\'(\'+q.replace(/[.*+?^${}()|[\\]\\\\]/g,\'\\\\$&\')+\')\',\'gi\'),\'<span class="highlight">$1</span>\')}'+
'function k(){var q=document.getElementById(\'s\').value;if(!q||!q.trim()){document.getElementById(\'st\').textContent=\'Shkruani një term\';return}'+
'document.getElementById(\'st\').textContent=\'Duke kërkuar...\';'+
'google.script.run.withSuccessHandler(function(r){if(r.error){document.getElementById(\'st\').textContent=r.error;return}'+
'var fr=r.results;if(fl){fr=r.results.filter(function(x){for(var k in x.data){var kk=k.toLowerCase();if(fl==\'emer\'&&(kk.includes(\'emer\')||kk.includes(\'emri\')))return 1;if(fl==\'mbiemer\'&&(kk.includes(\'mbiemer\')||kk.includes(\'mbiemri\')))return 1;if(fl==\'nr\'&&(kk.includes(\'nr\')||kk.includes(\'personal\')||kk.includes(\'legjitim\')||kk.includes(\'letër\')))return 1;if(fl==\'telefon\'&&(kk.includes(\'telefon\')||kk.includes(\'tel\')))return 1;if(fl==\'email\'&&(kk.includes(\'email\')||kk.includes(\'mail\')))return 1}return 0})}'+
'document.getElementById(\'st\').innerHTML=\'<b>\'+fr.length+\'</b> rezultate nga \'+r.total+\' — "\'+r.query+\'"\';'+
'var htm=\'\';fr.forEach(function(x){htm+=\'<div class="result-item" onclick="g(\\\'\'+x.sheetName.replace(/\'/g,"\\\\\'")+\'\\\',\'+x.rowIndex+\')">\'+'+
'\'<div class="sheet-name">📄 \'+x.sheetName+\' — Rreshti \'+x.rowIndex+\'</div>\';'+
'for(var k in x.data){htm+=\'<div class="field"><span class="label">\',+k+\':</span> <span class="value">\'+h(x.data[k],q)+\'</span></div>\'}'+
'htm+=\'</div>\'};document.getElementById(\'res\').innerHTML=htm})'+
'.withFailureHandler(function(e){document.getElementById(\'st\').textContent=\'Gabim: \'+e.message})'+
'.searchAllSheets(q)}'+
'function g(sn,ri){google.script.run.activateSheetAndRow(sn,ri);google.script.host.close()}'+
'</script></body></html>';

var REPORT_HTML = '<!DOCTYPE html><html><head><base target="_top"><style>'+
'body{font-family:Segoe UI;background:#1e1e23;color:#c8cdd6;padding:16px}'+
'.controls{display:flex;gap:8px;margin-bottom:12px;flex-wrap:wrap}'+
'.controls input{flex:1;min-width:200px;padding:8px;border:1px solid #3a3c44;border-radius:4px;background:#2a2c33;color:#fff}'+
'.controls select{padding:8px;border:1px solid #3a3c44;border-radius:4px;background:#2a2c33;color:#fff}'+
'.controls button{padding:8px 16px;background:#0064b0;color:#fff;border:none;border-radius:4px;cursor:pointer}'+
'.controls button.export{background:#2d7d46}'+
'.stats{color:#6a7a8a;font-size:12px}'+
'table{width:100%;border-collapse:collapse;font-size:12px}'+
'th{background:#003d66;color:#fff;padding:6px;text-align:left;cursor:pointer;position:sticky;top:0}'+
'td{padding:5px 8px;border-bottom:1px solid #2a2c33}'+
'tr:nth-child(even){background:#22242b}tr:hover{background:#2a2d36}'+
'.scrollable{max-height:480px;overflow:auto;border:1px solid #333;border-radius:4px}'+
'.sheet-tab{padding:6px 14px;background:#25272e;border:1px solid #333;display:inline-block;cursor:pointer;font-size:12px;color:#8ab}'+
'.sheet-tab.active{background:#003d66;color:#fff}'+
'</style></head><body><h2>Raporto</h2>'+
'<div class="controls">'+
'<input id="fs" placeholder="Filtro..." oninput="rf()">'+
'<select id="ss" onchange="sw()"></select>'+
'<button onclick="rf()">Filtro</button>'+
'<button class="export" onclick="ex()">Export CSV</button>'+
'<button onclick="window.print()">Printo</button></div>'+
'<div id="st" class="stats">Duke ngarkuar...</div>'+
'<div id="tabs"></div>'+
'<div class="scrollable"><table><thead id="th"></thead><tbody id="tb"></tbody></table></div>'+
'<script>var ad=[];'+
'function ld(){google.script.run.withSuccessHandler(function(d){ad=d;var sel=document.getElementById(\'ss\');sel.innerHTML=\'\';var tabs=document.getElementById(\'tabs\');tabs.innerHTML=\'\';d.forEach(function(s,i){var o=document.createElement(\'option\');o.value=i;o.textContent=s.name+\' (\'+s.totalRows+\')\';sel.appendChild(o);var t=document.createElement(\'span\');t.className=\'sheet-tab\'+(i==0?\' active\':\'\');t.textContent=s.name;t.onclick=function(){document.querySelectorAll(\'.sheet-tab\').forEach(function(x){x.classList.remove(\'active\')});t.classList.add(\'active\');sel.value=i;rs(s)};tabs.appendChild(t)});if(d.length)rs(d[0])}).getSpreadsheetData()}'+
'function sw(){var i=parseInt(document.getElementById(\'ss\').value);document.querySelectorAll(\'.sheet-tab\').forEach(function(t,idx){t.classList.toggle(\'active\',idx==i)});rs(ad[i])}'+
'function rs(s){if(!s)return;var q=document.getElementById(\'fs\').value.toLowerCase().trim();var fr=s.rows;if(q)fr=s.rows.filter(function(r){return r.some(function(c){return String(c).toLowerCase().includes(q)})})'+
'document.getElementById(\'st\').innerHTML=\'<b>\'+fr.length+\'</b> nga \'+s.totalRows+\' — "\'+s.name+\'"\'+(q?\' — "\'+q+\'"\':\'\')'+
'var th=document.getElementById(\'th\'),tb=document.getElementById(\'tb\');th.innerHTML=\'<tr>\'+s.headers.map(function(h){return\'<th onclick="st(this,\\\'\'+h.replace(/\'/g,"\\\\\'")+\'\\\')">\'+h+\'</th>\'}).join(\'\')+\'</tr>\''+
'tb.innerHTML=\'\';fr.forEach(function(r){var tr=document.createElement(\'tr\');r.forEach(function(c){var td=document.createElement(\'td\');td.textContent=c;td.title=c;tr.appendChild(td)});tb.appendChild(tr)})}'+
'function rf(){var i=parseInt(document.getElementById(\'ss\').value);rs(ad[i])}'+
'function st(th,k){var i=parseInt(document.getElementById(\'ss\').value),s=ad[i],ci=s.headers.indexOf(k);if(ci<0)return;var asc=th._sa===undefined?true:!th._sa;th._sa=asc'+
's.rows.sort(function(a,b){var va=String(a[ci]||\'\'),vb=String(b[ci]||\'\'),na=parseFloat(va.replace(\',\',\'.\')),nb=parseFloat(vb.replace(\',\',\'.\'));if(!isNaN(na)&&!isNaN(nb))return asc?na-nb:nb-na;return asc?va.localeCompare(vb):vb.localeCompare(va)});rs(s)}'+
'function ex(){var i=parseInt(document.getElementById(\'ss\').value),s=ad[i];if(!s)return;var c=s.headers.join(\',\')+\'\\n\';s.rows.forEach(function(r){c+=r.map(function(v){var s=String(v).replace(/"/g,\'\'\');return s.includes(\',\')||s.includes(\'"\')?\'"\'+s+\'"\':s}).join(\',\')+\'\\n\'});var b=new Blob([c],{type:\'text/csv;charset=utf-8;\'}),a=document.createElement(\'a\');a.href=URL.createObjectURL(b);a.download=s.name+\'_raport.csv\';a.click()}'+
'window.onload=ld</script></body></html>';
