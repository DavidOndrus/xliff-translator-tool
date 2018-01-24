# XLIFF Translator Tool
Simple XLIFF (\*.xlf/\*.xliff 1.2 & 2.0) editor with merge/import feature

I made this tool mainly for *Angular* translations because I couldn't find any simple, suitable and free tool. It's compatible with XLIFF 1.2 and XLIFF 2.0 and you can even combine them (open 1.2, import 2.0).

With this tool you can simply manage and update your translation files. Open old translations file, import newly generated file. Old translations won't be touched and new translations will be added to the list. Write translations in "Target" column, then "Save as..." file. Columns are resizable, reorderable and sortable.

EXAMPLE: Opening "strings.en.xlf" with 4 translation units and then importing "strings.en.xlf" (same file) will lead to the same list of 4 translation units not changed by import.

WARNING: This tool also removes all unnecessary elements from XLIFF file such as "location" of translation in source code.

DOWNLOAD: [Releases](https://github.com/DavidOndrus/xliff-translator-tool/releases)

## Images
![table image](Images/table.png)
![menu image](Images/menu.png)

## Output
SOURCE:
```XML
<?xml version="1.0" encoding="UTF-8" ?>
<xliff version="1.2" xmlns="urn:oasis:names:tc:xliff:document:1.2">
  <file source-language="en" datatype="plaintext" original="ng2.template">
    <body>
      <trans-unit id="loginHeaderTitle" datatype="html">
        <source>Login</source>
        <target>Login</target>
        <context-group purpose="location">
          <context context-type="sourcefile">app/login/login.component.ts</context>
          <context context-type="linenumber">7</context>
        </context-group>
        <note priority="1" from="description">description</note>
        <note priority="1" from="meaning">meaning</note>
      </trans-unit>
    </body>
  </file>
</xliff>
```
RESULT:
```XML
<?xml version="1.0" encoding="UTF-8"?>
<xliff version="1.2" xmlns="urn:oasis:names:tc:xliff:document:1.2">
  <file source-language="en">
    <body>
      <trans-unit id="loginHeaderTitle">
        <source>Login</source>
        <target>Login</target>
        <note from="description">description</note>
        <note from="meaning">meaning</note>
      </trans-unit>
    </body>
  </file>
</xliff>
```

### Angular i18n
Generate XLIFF file with Angular xi18n and open it with this tool
```
ng xi18n --i18nFormat=xlf --outputPath src/locales --outFile strings.en.xlf --locale en
```
or
```
ng xi18n --i18nFormat=xlf2 --outputPath src/locales --outFile strings.en.xlf --locale en
```
