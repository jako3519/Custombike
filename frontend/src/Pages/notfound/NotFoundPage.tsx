import "../service/ServicePage.css";
import { useText } from "../../texts/TextContext";
import RichText from "../../texts/RichText";
import EditButton from "../../admin/EditButton";
import Footer from "../../Components/footer/Footer";
import PageBackdrop from "../../Components/backdrop/PageBackdrop";

// Vises (med status 404 fra serveren) for en ydelse, der ikke findes
function NotFoundPage() {
  const title = useText("notFound.title");
  const text = useText("notFound.text");
  const back = useText("notFound.back");

  return (
    <div className="service-page notfound-page" id="top">
      <PageBackdrop />
      <header className="service-hero">
        <div className="content-container service-hero-inner">
          <h1>
            {title}
            <EditButton ids={["notFound.title", "notFound.text", "notFound.back"]} label="Rediger tekst" />
          </h1>
        </div>
      </header>
      <div className="service-content glass-sheet">
        <div className="content-container">
          <RichText text={text} className="notfound-text" />
          <a className="notfound-home" href="/">
            {back}
          </a>
        </div>
      </div>
      <Footer />
    </div>
  );
}

export default NotFoundPage;
