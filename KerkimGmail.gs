function onOpen() {
  var ui = SpreadsheetApp.getUi();
  ui.createMenu('Kërko Në Gmail')
    .addItem('Kërko në Inbox', 'showGmailSearch')
    .addItem('Kërko me datë', 'showGmailDateSearch')
    .addSeparator()
    .addItem('Raporto Email', 'showGmailReport')
    .addToUi();
}

function showGmailSearch() {
  var html = HtmlService.createHtmlOutput(GMAIL_SEARCH_HTML).setWidth(600).setHeight(500);
  SpreadsheetApp.getUi().showSidebar(html);
}

function showGmailDateSearch() {
  var html = HtmlService.createHtmlOutput(GMAIL_DATE_HTML).setWidth(400).setHeight(350);
  SpreadsheetApp.getUi().showModalDialog(html, 'Kërko me datë');
}

function showGmailReport() {
  var html = HtmlService.createHtmlOutput(GMAIL_REPORT_HTML).setWidth(950).setHeight(600);
  SpreadsheetApp.getUi().showModalDialog(html, 'Raporto Email');
}

function searchGmail(query) {
  if (!query || !query.trim()) return { error: 'Shkruani një term kërkimi.' };
  try {
    var threads = GmailApp.search(query, 0, 50);
    var results = [];
    threads.forEach(function(thread) {
      var messages = thread.getMessages();
      messages.forEach(function(msg) {
        results.push({
          from: msg.getFrom(),
          subject: msg.getSubject(),
          date: Utilities.formatDate(msg.getDate(), Session.getScriptTimeZone(), 'dd.MM.yyyy HH:mm'),
          body: msg.getPlainBody().substring(0, 300),
          id: msg.getId(),
          threadId: thread.getId()
        });
      });
    });
    return { results: results, total: results.length, query: query };
  } catch(e) {
    return { error: e.toString() };
  }
}

function searchGmailDate(fromDate, toDate, keyword) {
  try {
    var q = '';
    if (fromDate) q += 'after:' + fromDate + ' ';
    if (toDate) q += 'before:' + toDate + ' ';
    if (keyword) q += keyword;
    var threads = GmailApp.search(q, 0, 50);
    var results = [];
    threads.forEach(function(thread) {
      var messages = thread.getMessages();
      messages.forEach(function(msg) {
        results.push({
          from: msg.getFrom(),
          subject: msg.getSubject(),
          date: Utilities.formatDate(msg.getDate(), Session.getScriptTimeZone(), 'dd.MM.yyyy HH:mm'),
          body: msg.getPlainBody().substring(0, 200),
          id: msg.getId()
        });
      });
    });
    return { results: results, total: results.length, query: q };
  } catch(e) {
    return { error: e.toString() };
  }
}

function getGmailReport() {
  try {
    var threads = GmailApp.getInboxThreads(0, 100);
    var stats = { total: 0, unread: 0, senders: {} };
    threads.forEach(function(t) {
      var msgs = t.getMessages();
      stats.total += msgs.length;
      if (t.isUnread()) stats.unread++;
      msgs.forEach(function(m) {
        var from = m.getFrom().replace(/<[^>]+>/g, '').trim();
        stats.senders[from] = (stats.senders[from] || 0) + 1;
      });
    });
    return stats;
  } catch(e) {
    return { error: e.toString() };
  }
}

function getGmailMessages() {
  try {
    var threads = GmailApp.getInboxThreads(0, 20);
    var msgs = [];
    threads.forEach(function(t) {
      t.getMessages().forEach(function(m) {
        msgs.push({
          from: m.getFrom(),
          subject: m.getSubject(),
          date: Utilities.formatDate(m.getDate(), Session.getScriptTimeZone(), 'dd.MM.yyyy HH:mm'),
          snippet: m.getPlainBody().substring(0, 150)
        });
      });
    });
    return msgs;
  } catch(e) {
    return { error: e.toString() };
  }
}

var GMAIL_SEARCH_HTML = '<!DOCTYPE html><html><head><base target="_top"><style>'+
'body{font-family:Segoe UI;background:#1e1e23;color:#c8cdd6;padding:12px;margin:0}'+
'.sb{display:flex;gap:8px;margin-bottom:10px}'+
'.sb input{flex:1;padding:8px;border:1px solid #3a3c44;border-radius:4px;background:#2a2c33;color:#fff;font-size:13px}'+
'.sb button{padding:8px 16px;background:#0064b0;color:#fff;border:none;border-radius:4px;cursor:pointer;font-size:13px}'+
'.item{background:#25272e;border:1px solid #333;border-radius:6px;padding:8px;margin-bottom:6px}'+
'.from{color:#4a8abc;font-weight:600;font-size:12px}'+
'.subj{color:#e0e5ee;font-size:13px;font-weight:500}'+
'.date{color:#6a7a8a;font-size:11px}'+
'.body{color:#9aa;font-size:11px;margin-top:4px;max-height:40px;overflow:hidden}'+
'.stats{color:#6a7a8a;font-size:12px;margin-bottom:8px}'+
'</style></head><body>'+
'<div class="sb"><input id="q" placeholder="Kërko: emër, subjekt, fjalë kyçe..." onkeyup="if(event.key==\'Enter\')k()">'+
'<button onclick="k()">Kërko</button></div>'+
'<div id="st" class="stats">Shkruani një term për të kërkuar në Inbox</div><div id="res"></div>'+
'<script>function k(){var q=document.getElementById("q").value;if(!q||!q.trim()){document.getElementById("st").textContent="Shkruani një term";return}'+
'document.getElementById("st").textContent="⏳ Duke kërkuar në Gmail...";document.getElementById("res").innerHTML="";'+
'google.script.run.withSuccessHandler(function(r){if(r.error){document.getElementById("st").textContent="Gabim: "+r.error;return}'+
'document.getElementById("st").innerHTML="<b>"+r.total+"</b> email-a për: \""+r.query+"\"";var h="";'+
'r.results.forEach(function(m){h+="<div class=\'item\'><div class=\'from\'>📧 "+m.from+"</div><div class=\'subj\'>"+m.subject+"</div>"+'+
'"<div class=\'date\'>"+m.date+"</div><div class=\'body\'>"+m.body+"</div></div>"});'+
'document.getElementById("res").innerHTML=h})'+
'.withFailureHandler(function(e){document.getElementById("st").textContent="Gabim: "+e.message})'+
'.searchGmail(q)}</script></body></html>';

var GMAIL_DATE_HTML = '<!DOCTYPE html><html><head><base target="_top"><style>'+
'body{font-family:Segoe UI;background:#1e1e23;color:#c8cdd6;padding:16px}'+
'label{font-size:12px;color:#8ab;display:block;margin-top:8px}'+
'input{padding:8px;border:1px solid #3a3c44;border-radius:4px;background:#2a2c33;color:#fff;width:100%}'+
'button{padding:8px 20px;background:#0064b0;color:#fff;border:none;border-radius:4px;cursor:pointer;margin-top:12px}'+
'.item{background:#25272e;border:1px solid #333;border-radius:6px;padding:8px;margin:4px 0;font-size:12px}'+
'.from{color:#4a8abc;font-weight:600}.subj{color:#e0e5ee}.date{color:#6a7a8a}'+
'</style></head><body><h3>Kërko me datë</h3>'+
'<label>Nga data (yyyy/mm/dd):</label><input id="fd" value="2025/01/01">'+
'<label>Deri më (yyyy/mm/dd):</label><input id="td" value="2026/12/31">'+
'<label>Fjalë kyçe:</label><input id="kw" placeholder="Opsionale...">'+
'<button onclick="k()">Kërko</button><div id="st" style="margin-top:8px;color:#6a7a8a"></div><div id="res" style="margin-top:8px"></div>'+
'<script>function k(){var fd=document.getElementById("fd").value,td=document.getElementById("td").value,kw=document.getElementById("kw").value;'+
'document.getElementById("st").textContent="⏳ Duke kërkuar...";'+
'google.script.run.withSuccessHandler(function(r){if(r.error){document.getElementById("st").textContent=r.error;return}'+
'document.getElementById("st").innerHTML="<b>"+r.total+"</b> email-a";var h="";'+
'r.results.forEach(function(m){h+="<div class=\'item\'><div class=\'from\'>"+m.from+"</div><div class=\'subj\'>"+m.subject+"</div><div class=\'date\'>"+m.date+"</div></div>"});'+
'document.getElementById("res").innerHTML=h})'+
'.withFailureHandler(function(e){document.getElementById("st").textContent="Gabim: "+e.message})'+
'.searchGmailDate(fd,td,kw)}</script></body></html>';

var GMAIL_REPORT_HTML = '<!DOCTYPE html><html><head><base target="_top"><style>'+
'body{font-family:Segoe UI;background:#1e1e23;color:#c8cdd6;padding:16px}'+
'.controls{display:flex;gap:8px;margin-bottom:12px}'+
'.controls button{padding:8px 16px;background:#0064b0;color:#fff;border:none;border-radius:4px;cursor:pointer}'+
'.controls button.export{background:#2d7d46}'+
'.stats{color:#6a7a8a;font-size:12px;margin-bottom:8px}'+
'table{width:100%;border-collapse:collapse;font-size:12px}'+
'th{background:#003d66;color:#fff;padding:6px;text-align:left;position:sticky;top:0}'+
'td{padding:5px 8px;border-bottom:1px solid #2a2c33;max-width:300px;overflow:hidden;text-overflow:ellipsis}'+
'tr:nth-child(even){background:#22242b}tr:hover{background:#2a2d36}'+
'.scrollable{max-height:450px;overflow:auto;border:1px solid #333}'+
'</style></head><body><h2>📧 Raporto Email - Inbox</h2>'+
'<div class="controls"><button onclick="ld()">Rifresko</button><button class="export" onclick="ex()">Export CSV</button></div>'+
'<div id="st" class="stats">Duke ngarkuar...</div>'+
'<div class="scrollable"><table><thead><tr><th>Nga</th><th>Subjekti</th><th>Data</th></tr></thead><tbody id="tb"></tbody></table></div>'+
'<script>function ld(){document.getElementById("st").textContent="⏳ Duke ngarkuar...";'+
'google.script.run.withSuccessHandler(function(r){if(r.error){document.getElementById("st").textContent=r.error;return}'+
'var tb=document.getElementById("tb");tb.innerHTML="";r.forEach(function(m){tb.innerHTML+="<tr><td>"+m.from+"</td><td>"+m.subject+"</td><td>"+m.date+"</td></tr>"});'+
'document.getElementById("st").innerHTML="<b>"+r.length+"</b> email-a në Inbox"})'+
'.withFailureHandler(function(e){document.getElementById("st").textContent="Gabim: "+e.message})'+
'.getGmailMessages()}'+
'function ex(){var rows=[];document.querySelectorAll("#tb tr").forEach(function(tr){var cells=[];tr.querySelectorAll("td").forEach(function(td){cells.push(td.textContent)});rows.push(cells.join(","))});'+
'var csv="Nga,Subjekti,Data\n"+rows.join("\n");var b=new Blob([csv],{type:"text/csv;charset=utf-8"});var a=document.createElement("a");a.href=URL.createObjectURL(b);a.download="inbox_raport.csv";a.click()}'+
'window.onload=ld</script></body></html>';
