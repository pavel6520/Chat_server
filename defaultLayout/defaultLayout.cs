using ConnectionWorker;

public class defaultLayout : LayoutWorker {
	public void Init() {
		Echo("<!doctype html><html>");
		IncludeLayout("head");
		Echo("<body>");
		IncludeLayout("header");
		Echo("<div id=\"content\" class=\"container-fluid\">");
		IncludeContent();
		Echo("</div>");
		Echo("<script src=\"/assets/js/func.js\"></script>");
		Echo("<script src=\"/assets/js/popper.min.js\"></script>");
		Echo("<script src=\"/assets/js/bootstrap.min.js\"></script>");
		Echo("<script src=\"/assets/js/jasny-bootstrap.min.js\"></script>");
		Echo("<!-- Yandex.Metrika counter --> <script type='text/javascript' > (function(m,e,t,r,i,k,a){m[i]=m[i]||function(){(m[i].a=m[i].a||[]).push(arguments)}; m[i].l=1*new Date();k=e.createElement(t),a=e.getElementsByTagName(t)[0],k.async=1,k.src=r,a.parentNode.insertBefore(k,a)}) (window, document, 'script', 'https://mc.yandex.ru/metrika/tag.js', 'ym'); ym(53603836, 'init', { clickmap:true, trackLinks:true, accurateTrackBounce:true, webvisor:true }); </script> <noscript><div><img src='https://mc.yandex.ru/watch/53603836' style='position:absolute; left:-9999px;' alt='' /></div></noscript> <!-- /Yandex.Metrika counter -->");
		Echo("</body></html>");
	}
}