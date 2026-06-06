/**
 * KërkimFiltër - Google Apps Script për kërkim dhe filtrim të raporteve
 * 
 * Përdorimi:
 * 1. Hapni Google Sheets (ose Google Docs)
 * 2. Shkoni te Extensions > Apps Script
 * 3. Kopjoni këtë kod dhe ruajeni
 * 4. Rifreskoni fletën - do të shfaqet menuja "KërkimFiltër"
 * 
 * Mund të kërkoni nga: Nr Personal, Emri, Mbiemri, ose çdo fushë tjetër
 */

function onOpen() {
  var ui = SpreadsheetApp.getUi();
  ui.createMenu('KërkimFiltër')
    .addItem('Kërko në të gjitha sheet-et', 'showSearchDialog')
    .addItem('Filtro sheet-in aktiv', 'filterActiveSheet')
    .addSeparator()
    .addItem('Fshi filtrat', 'clearFilters')
    .addItem('Raporto (HTML)', 'showReportDialog')
    .addToUi();
}

function showSearchDialog() {
  var html = HtmlService.createHtmlOutputFromFile('SearchDialog')
    .setWidth(500)
    .setHeight(400)
    .setTitle('Kërko në Raporte');
  SpreadsheetApp.getUi().showModalDialog(html, '🔍 Kërko në të Gjitha Sheet-et');
}

function showReportDialog() {
  var html = HtmlService.createHtmlOutputFromFile('ReportViewer')
    .setWidth(900)
    .setHeight(600)
    .setTitle('Raporto - Rezultatet');
  SpreadsheetApp.getUi().showModalDialog(html, '📊 Raporto');
}

function filterActiveSheet() {
  var sheet = SpreadsheetApp.getActiveSheet();
  var range = sheet.getDataRange();
  var values = range.getValues();
  if (values.length < 2) return;
  
  var headers = values[0];
  var data = values.slice(1);
  
  var html = HtmlService.createHtmlOutput(
    '<style>' +
    'body{font-family:Segoe UI,sans-serif;background:#1e1e23;color:#c8cdd6;padding:16px}' +
    'input,select{width:100%;padding:8px;margin:6px 0 12px;border:1px solid #444;border-radius:4px;background:#2a2c33;color:#fff}' +
    'button{padding:8px 20px;background:#0064b0;color:#fff;border:none;border-radius:4px;cursor:pointer}' +
    'button:hover{background:#0080d0}' +
    'table{width:100%;border-collapse:collapse;margin-top:12px}' +
    'th{background:#003d66;color:#fff;padding:6px;text-align:left}' +
    'td{padding:6px;border-bottom:1px solid #333}' +
    'tr:hover{background:#2a2c33}' +
    '.count{color:#8ab;margin:8px 0}' +
    '</style>' +
    '<h2>Filtro: ' + sheet.getName() + '</h2>' +
    '<div style="display:grid;grid-template-columns:1fr 1fr;gap:8px">' +
    headers.map(function(h, i) {
      return '<label>' + h + '<input type="text" id="f_' + i + '" placeholder="Kërko..."></label>';
    }).join('') +
    '</div>' +
    '<button onclick="filterData()">Filtro</button> ' +
    '<button onclick="clearAll()">Pastro</button>' +
    '<div id="count" class="count">Total: ' + data.length + ' rreshta</div>' +
    '<div id="results"></div>' +
    '<script>' +
    'var headers = ' + JSON.stringify(headers) + ';' +
    'var data = ' + JSON.stringify(data) + ';' +
    'function filterData(){' +
    'var filters = headers.map(function(h,i){return document.getElementById("f_"+i).value.toLowerCase()});' +
    'var filtered = data.filter(function(row){return filters.every(function(f,i){return !f || String(row[i]).toLowerCase().includes(f)})});' +
    'document.getElementById("count").textContent = "Gjetur: "+filtered.length+" nga "+data.length+" rreshta";' +
    'var html = "<table><tr><th>"+headers.join("</th><th>")+"</th></tr>";' +
    'filtered.forEach(function(r){html += "<tr><td>"+r.join("</td><td>")+"</td></tr>"});' +
    'html += "</table>";' +
    'document.getElementById("results").innerHTML = html;' +
    '}' +
    'function clearAll(){headers.forEach(function(h,i){document.getElementById("f_"+i).value=""});filterData();}' +
    'window.onload = function(){filterData();};' +
    '</script>'
  ).setWidth(950).setHeight(650).setTitle('Filtro - ' + sheet.getName());
  SpreadsheetApp.getUi().showModalDialog(html, '🔍 Filtro');
}

function clearFilters() {
  var sheet = SpreadsheetApp.getActiveSheet();
  if (sheet.getFilter()) {
    sheet.getFilter().remove();
    SpreadsheetApp.getUi().alert('Filtrat u fshinë.');
  } else {
    SpreadsheetApp.getUi().alert('Nuk ka filtra aktivë.');
  }
}

function searchAllSheets(query) {
  if (!query || query.trim() === '') return { error: 'Shkruani një term kërkimi.' };
  
  var q = query.toLowerCase().trim();
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheets = ss.getSheets();
  var results = [];
  var totalRows = 0;
  
  for (var s = 0; s < sheets.length; s++) {
    var sheet = sheets[s];
    var name = sheet.getName();
    var range = sheet.getDataRange();
    var values = range.getValues();
    if (values.length < 2) continue;
    
    var headers = values[0];
    var data = values.slice(1);
    
    for (var r = 0; r < data.length; r++) {
      var row = data[r];
      var match = false;
      for (var c = 0; c < row.length; c++) {
        if (String(row[c]).toLowerCase().includes(q)) {
          match = true;
          break;
        }
      }
      if (match) {
        var rowData = {};
        for (var c = 0; c < headers.length; c++) {
          rowData[headers[c]] = row[c];
        }
        results.push({
          sheetName: name,
          rowIndex: r + 2,
          data: rowData
        });
        totalRows++;
      }
    }
  }
  
  return {
    results: results,
    total: totalRows,
    sheets: sheets.length,
    query: query,
    timestamp: new Date().toLocaleString()
  };
}

function getSpreadsheetData() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheets = ss.getSheets();
  var allData = [];
  
  for (var s = 0; s < sheets.length; s++) {
    var sheet = sheets[s];
    var range = sheet.getDataRange();
    var values = range.getValues();
    if (values.length < 2) continue;
    
    allData.push({
      name: sheet.getName(),
      headers: values[0],
      rows: values.slice(1),
      totalRows: values.length - 1
    });
  }
  return allData;
}

function activateSheetAndRow(sheetName, rowIndex) {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheet = ss.getSheetByName(sheetName);
  if (sheet) {
    ss.setActiveSheet(sheet);
    sheet.setActiveRange(sheet.getRange(rowIndex, 1, 1, sheet.getLastColumn()));
  }
}
