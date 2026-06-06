function onHomepage(e) {
  return buildMainCard();
}

function onGmailSidebar(e) {
  return buildMainCard();
}

function buildMainCard() {
  var section = CardService.newCardSection();

  var searchInput = CardService.newTextInput()
    .setFieldName('query')
    .setTitle('Kërko në email')
    .setHint('Fjalë kyçe, emër, subjekt...');

  var searchButton = CardService.newTextButton()
    .setText('Kërko')
    .setOnClickAction(CardService.newAction().setFunctionName('handleSearch'));

  section.addWidget(searchInput);
  section.addWidget(CardService.newButtonSet().addButton(searchButton));

  section.addWidget(CardService.newTextParagraph()
    .setText('<b>────────── OSE ──────────</b>'));

  var fileInput = CardService.newTextInput()
    .setFieldName('fileName')
    .setTitle('Kërko file/bashkëngjitje')
    .setHint('Emri i file-it (pdf, word, excel...)');

  var fileButton = CardService.newTextButton()
    .setText('Kërko File')
    .setOnClickAction(CardService.newAction().setFunctionName('handleFileSearch'));

  section.addWidget(fileInput);
  section.addWidget(CardService.newButtonSet().addButton(fileButton));

  var card = CardService.newCardBuilder()
    .setName('mainCard')
    .setHeader(CardService.newCardHeader()
      .setTitle('🔍 Kërko Në Gmail')
      .setSubtitle('Kërko email ose bashkëngjitje'));

  var footerSection = CardService.newCardSection();
  footerSection.addWidget(CardService.newTextParagraph()
    .setText('<font color="#888">Kërkimi bëhet në të gjithë inbox-in tuaj.</font>'));

  card.addSection(section);
  card.addSection(footerSection);

  return card.build();
}

function handleSearch(e) {
  var query = e.formInput.query;
  if (!query || !query.trim()) {
    return CardService.newActionResponseBuilder()
      .setNotification(CardService.newNotification().setText('Shkruani një term kërkimi.'))
      .build();
  }

  try {
    var threads = GmailApp.search(query, 0, 25);
    var section = CardService.newCardSection()
      .setHeader('📧 Rezultatet: ' + threads.length + ' email');

    if (threads.length === 0) {
      section.addWidget(CardService.newTextParagraph()
        .setText('Nuk u gjet asgjë për: "' + query + '"'));
    } else {
      for (var i = 0; i < threads.length; i++) {
        var t = threads[i];
        var msg = t.getMessages()[0];
        var from = msg.getFrom().replace(/<[^>]+>/g, '').trim();
        var subject = msg.getSubject() || '(pa subjekt)';
        var date = Utilities.formatDate(msg.getDate(), Session.getScriptTimeZone(), 'dd.MM.yyyy HH:mm');
        var snippet = msg.getPlainBody().substring(0, 120).replace(/\n/g, ' ');
        var link = 'https://mail.google.com/mail/u/0/#inbox/' + t.getId();

        section.addWidget(CardService.newDecoratedText()
          .setText('<b>' + from + '</b><br><a href="' + link + '" target="_blank">' + subject + '</a><br>' +
            '<font color="#999">' + date + '</font><br>' +
            '<font color="#aaa">' + snippet + '...</font>')
          .setWrapText(true));
        section.addWidget(CardService.newDivider());
      }
    }

    var card = CardService.newCardBuilder()
      .setName('searchResults')
      .setHeader(CardService.newCardHeader()
        .setTitle('🔍 "' + query + '"')
        .setSubtitle(threads.length + ' rezultate'))
      .addSection(section)
      .addSection(buildBackSection());

    return card.build();
  } catch (err) {
    return CardService.newActionResponseBuilder()
      .setNotification(CardService.newNotification().setText('Gabim: ' + err.message))
      .build();
  }
}

function handleFileSearch(e) {
  var fileName = e.formInput.fileName;
  if (!fileName || !fileName.trim()) {
    return CardService.newActionResponseBuilder()
      .setNotification(CardService.newNotification().setText('Shkruani emrin e file-it.'))
      .build();
  }

  try {
    var query = 'filename:' + fileName;
    var threads = GmailApp.search(query, 0, 25);
    var section = CardService.newCardSection()
      .setHeader('📎 File të gjetura: ' + threads.length);

    if (threads.length === 0) {
      section.addWidget(CardService.newTextParagraph()
        .setText('Nuk u gjet asnjë file me emrin: "' + fileName + '"'));
    } else {
      for (var i = 0; i < threads.length; i++) {
        var t = threads[i];
        var msg = t.getMessages()[0];
        var from = msg.getFrom().replace(/<[^>]+>/g, '').trim();
        var subject = msg.getSubject() || '(pa subjekt)';
        var date = Utilities.formatDate(msg.getDate(), Session.getScriptTimeZone(), 'dd.MM.yyyy');
        var attachments = msg.getAttachments();
        var link = 'https://mail.google.com/mail/u/0/#inbox/' + t.getId();

        var fileList = '';
        for (var j = 0; j < attachments.length; j++) {
          var att = attachments[j];
          var attName = att.getName().toLowerCase();
          if (attName.includes(fileName.toLowerCase())) {
            fileList += '📎 ' + att.getName() + ' (' + Math.round(att.getSize() / 1024) + ' KB)<br>';
          }
        }

        section.addWidget(CardService.newDecoratedText()
          .setText('<b>' + from + '</b><br><a href="' + link + '" target="_blank">' + subject + '</a><br>' +
            '<font color="#999">' + date + '</font><br>' +
            '<font color="#4CAF50">' + fileList + '</font>')
          .setWrapText(true));
        section.addWidget(CardService.newDivider());
      }
    }

    var card = CardService.newCardBuilder()
      .setName('fileResults')
      .setHeader(CardService.newCardHeader()
        .setTitle('📎 "' + fileName + '"')
        .setSubtitle(threads.length + ' email me file'))
      .addSection(section)
      .addSection(buildBackSection());

    return card.build();
  } catch (err) {
    return CardService.newActionResponseBuilder()
      .setNotification(CardService.newNotification().setText('Gabim: ' + err.message))
      .build();
  }
}

function buildBackSection() {
  var section = CardService.newCardSection();
  var backButton = CardService.newTextButton()
    .setText('← Kërko përsëri')
    .setOnClickAction(CardService.newAction().setFunctionName('onHomepage'));
  section.addWidget(CardService.newButtonSet().addButton(backButton));
  return section;
}
